# Builds the two xpera schedules for ProjectLibre (Microsoft Project XML), with Davao rates.
#   1 - My current timeline  : your own plan, 7-day weeks (weekends worked)
#   2 - Realistic estimate   : a slower real-world plan, Monday to Friday, ending in March
# Run:  python make-schedules.py      then open each .xml in ProjectLibre and Save As .pod.
# Change the hours, rates, or START below and run it again.
from datetime import date, datetime, timedelta
from xml.sax.saxutils import escape

START = date(2026, 10, 30)

# Pesos per hour (typical junior IT pay in Davao City, about ₱19,000-₱27,000 a month).
# Overtime is 125% of the rate, as the Labor Code requires on ordinary working days.
PEOPLE = {'Nicho FD': 130, 'Nicho BD': 150, 'Nicho QA': 110}
# Things paid once, not by the hour ("Cost per use" in ProjectLibre).
ONE_TIME = {'Hosting': 1500, 'Domain': 800, 'Cloud Database': 1800}

USE_CASES = ['UC-01 Manage Staff Accounts', 'UC-02 Manage Products', 'UC-03 Manage Customer Records',
             'UC-04 Record Sale', 'UC-05 Record Payment', 'UC-06 Manage Inquiries', 'UC-07 Manage Complaints',
             'UC-08 Record Feedback', 'UC-09 View BI Dashboard', 'UC-10 Generate Operational Report',
             'UC-11 Manage Retention Campaigns', 'UC-12 Manage Branches', 'UC-13 Manage Tenants',
             'UC-14 Manage Subscriptions', 'UC-15 View Platform Insights']

# Hours per use case: (front-end, back-end, QA).
# Your current timeline, exactly as you edited it in ProjectLibre.
CURRENT = [(2.4, 1.6, 1.28), (2.4, 1.6, 1.12), (3.04, 2.8, 1.44), (3.2, 2.8, 1.12), (2.4, 2.4, 2.0),
           (2.0, 1.6, 0.96), (2.4, 0.96, 0.16), (2.64, 1.2, 0.32), (8, 8, 2), (4, 4, 2), (8, 8, 4),
           (24, 24, 8), (24, 24, 8), (12, 12, 4), (12, 12, 4)]
# The realistic estimate: what a junior developer working alone, Monday to Friday, really needs,
# including learning time for SQL Server, charts, and reports. It ends in mid-March.
REALISTIC = [(16, 12, 8), (16, 12, 8), (24, 16, 8), (36, 32, 12), (16, 16, 8), (16, 12, 8), (16, 12, 8),
             (12, 8, 4), (36, 36, 12), (20, 24, 8), (36, 32, 12), (24, 24, 12), (24, 24, 12), (16, 16, 8),
             (20, 16, 8)]

PLANS = {
    '1 - My current timeline.xml': dict(hours=CURRENT, seven_days=True,
        extra_after_uc11=[],   # Fix and Polish was removed; UC-12 now follows UC-11 directly
        testing=('Testing', [('Final Testing', 24, 'Nicho QA')])),
    '2 - Realistic estimate.xml': dict(hours=REALISTIC, seven_days=False,
        extra_after_uc11=[],   # Fix and Polish was removed; UC-12 now follows UC-11 directly
        testing=('Testing and Demo Data', [('Seed 200+ Transactions', 24, 'Nicho BD'), ('Final Testing', 32, 'Nicho QA')])),
}


def build(file_name, hours, seven_days, extra_after_uc11, testing):
    def working(d):
        return seven_days or d.weekday() < 5

    # A clock that walks through working time, 8-12 and 1-5, so start and finish dates are right.
    day, used = START, 0.0
    while not working(day):
        day += timedelta(days=1)

    def stamp(at_end=False):
        nonlocal day, used
        if at_end and used == 0:
            d = day - timedelta(days=1)
            while not working(d):
                d -= timedelta(days=1)
            return datetime(d.year, d.month, d.day, 17)
        clock = 8 + used if used < 4 or (used == 4 and at_end) else 9 + used
        return datetime(day.year, day.month, day.day, int(clock), int(round((clock % 1) * 60)) % 60)

    def advance(h):
        nonlocal day, used
        total = used + h
        while total >= 8 - 1e-9:
            total -= 8
            day += timedelta(days=1)
            while not working(day):
                day += timedelta(days=1)
        used = total

    tasks = []

    def add(name, level, h=0.0, who=None, pred=None, summary=False):
        t = dict(uid=len(tasks) + 1, name=name, level=level, hours=h, who=who, pred=pred, summary=summary)
        tasks.append(t)
        return t

    def work(name, level, h, who, prev):
        t = add(name, level, h, who, prev['uid'] if prev else None)
        t['start'] = stamp()
        advance(h)
        t['finish'] = stamp(at_end=True)
        return t

    def close(group, kids):
        group['start'], group['finish'] = kids[0]['start'], kids[-1]['finish']
        group['hours'] = sum(k['hours'] for k in kids)

    root = add('xpera CRM', 1, summary=True)
    prev, top = None, []
    for i, name in enumerate(USE_CASES):
        uc = add(name, 2, summary=True)
        kids = []
        for part, h, who in zip(('Front-End', 'Back-End', 'Quality Assurance'), hours[i], ('Nicho FD', 'Nicho BD', 'Nicho QA')):
            prev = work(part, 3, h, who, prev)
            kids.append(prev)
        close(uc, kids)
        top.append(uc)
        if name.startswith('UC-11'):
            for n, h, who in extra_after_uc11:
                prev = work(n, 2, h, who, prev)
                top.append(prev)
    group = add(testing[0], 2, summary=True)
    kids = []
    for n, h, who in testing[1]:
        prev = work(n, 3, h, who, prev)
        kids.append(prev)
    close(group, kids)
    top.append(group)
    close(root, top)
    # Paid on day one, like buying a domain before building.
    for n in ONE_TIME:
        t = add(n, 2, 8, n)
        t['start'] = datetime(START.year, START.month, START.day, 8)
        t['finish'] = datetime(START.year, START.month, START.day, 17)

    # Outline numbers like 1, 1.2, 1.2.3
    counters = [0] * 5
    for t in tasks:
        counters[t['level'] - 1] += 1
        for j in range(t['level'], 5):
            counters[j] = 0
        t['wbs'] = '.'.join(str(c) for c in counters[:t['level']])

    ts = lambda d: d.strftime('%Y-%m-%dT%H:%M:%S')
    def dur(h):
        minutes = int(round(h * 60))
        return f'PT{minutes // 60}H{minutes % 60}M0S'

    cal_name = 'xpera 7-day week' if seven_days else 'xpera Mon-Fri'
    x = ['<?xml version="1.0" encoding="UTF-8" standalone="yes"?>',
         '<Project xmlns="http://schemas.microsoft.com/project">',
         '<Name>xpera CRM</Name><Title>xpera CRM</Title><ScheduleFromStart>1</ScheduleFromStart>',
         f'<StartDate>{ts(root["start"])}</StartDate><FinishDate>{ts(root["finish"])}</FinishDate>',
         '<CurrencyDigits>2</CurrencyDigits><CurrencySymbol>₱</CurrencySymbol><CurrencyCode>PHP</CurrencyCode>',
         '<CalendarUID>1</CalendarUID><DefaultStartTime>08:00:00</DefaultStartTime><DefaultFinishTime>17:00:00</DefaultFinishTime>',
         f'<MinutesPerDay>480</MinutesPerDay><MinutesPerWeek>{3360 if seven_days else 2400}</MinutesPerWeek><DaysPerMonth>20</DaysPerMonth>',
         f'<Calendars><Calendar><UID>1</UID><Name>{cal_name}</Name><IsBaseCalendar>1</IsBaseCalendar><WeekDays>']
    for n in range(1, 8):          # 1 = Sunday ... 7 = Saturday
        if not seven_days and n in (1, 7):
            x.append(f'<WeekDay><DayType>{n}</DayType><DayWorking>0</DayWorking></WeekDay>')
        else:
            x.append(f'<WeekDay><DayType>{n}</DayType><DayWorking>1</DayWorking><WorkingTimes>'
                     '<WorkingTime><FromTime>08:00:00</FromTime><ToTime>12:00:00</ToTime></WorkingTime>'
                     '<WorkingTime><FromTime>13:00:00</FromTime><ToTime>17:00:00</ToTime></WorkingTime>'
                     '</WorkingTimes></WeekDay>')
    x.append('</WeekDays></Calendar></Calendars><Tasks>')
    for t in tasks:
        x.append(f'<Task><UID>{t["uid"]}</UID><ID>{t["uid"]}</ID><Name>{escape(t["name"])}</Name><Type>1</Type><IsNull>0</IsNull>'
                 f'<WBS>{t["wbs"]}</WBS><OutlineNumber>{t["wbs"]}</OutlineNumber><OutlineLevel>{t["level"]}</OutlineLevel>'
                 f'<Start>{ts(t["start"])}</Start><Finish>{ts(t["finish"])}</Finish><Duration>{dur(t["hours"])}</Duration>'
                 f'<DurationFormat>7</DurationFormat><Summary>{int(t["summary"])}</Summary><Milestone>0</Milestone>'
                 f'<ConstraintType>0</ConstraintType><CalendarUID>-1</CalendarUID>')
        if t['pred']:
            x.append(f'<PredecessorLink><PredecessorUID>{t["pred"]}</PredecessorUID><Type>1</Type></PredecessorLink>')
        x.append('</Task>')
    x.append('</Tasks><Resources>')
    resource_uid = {}
    for n, rate in list(PEOPLE.items()) + [(k, 0) for k in ONE_TIME]:
        u = len(resource_uid) + 1
        resource_uid[n] = u
        x.append(f'<Resource><UID>{u}</UID><ID>{u}</ID><Name>{n}</Name><Type>1</Type><MaxUnits>1.00</MaxUnits>'
                 f'<StandardRate>{rate}</StandardRate><StandardRateFormat>2</StandardRateFormat>'
                 f'<OvertimeRate>{round(rate * 1.25, 2)}</OvertimeRate><OvertimeRateFormat>2</OvertimeRateFormat>'
                 f'<CostPerUse>{ONE_TIME.get(n, 0)}</CostPerUse><CalendarUID>1</CalendarUID></Resource>')
    x.append('</Resources><Assignments>')
    a, cost = 0, 0.0
    for t in tasks:
        if t['who']:
            a += 1
            x.append(f'<Assignment><UID>{a}</UID><TaskUID>{t["uid"]}</TaskUID><ResourceUID>{resource_uid[t["who"]]}</ResourceUID>'
                     f'<Units>1</Units><Work>{dur(t["hours"])}</Work><Start>{ts(t["start"])}</Start><Finish>{ts(t["finish"])}</Finish></Assignment>')
            cost += t['hours'] * PEOPLE.get(t['who'], 0) + ONE_TIME.get(t['who'], 0)
    x.append('</Assignments></Project>')
    open(file_name, 'w', encoding='utf-8').write('\n'.join(x))
    print(f'{file_name}: {root["start"]:%b %d %Y} to {root["finish"]:%b %d %Y}, about ₱{cost:,.0f}')


for name, plan in PLANS.items():
    build(name, **plan)
