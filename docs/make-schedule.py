# Builds docs/xpera-schedule.xml (Microsoft Project XML), which ProjectLibre can open.
# Change START or the day counts below, then run this file again.
from datetime import date, datetime, timedelta
from pathlib import Path
from xml.sax.saxutils import escape

START = date(2026, 10, 31)
HOLIDAYS = set()  # ProjectLibre ignores holidays in this file; add them in File > Calendar
FD, BD, QA = 1, 2, 3
RESOURCES = {FD: 'Nicho FD', BD: 'Nicho BD', QA: 'Nicho QA', 4: 'Hosting', 5: 'Domain', 6: 'Cloud Database'}

# (group, [(use case, front-end days, back-end days, QA days)])
# Days are calendar days: the plan works weekends up to Lab 2 checking on Nov 19.
PLAN = [
    ('Lab 1 · Tenant A', [
        ('UC-01 Manage Staff Accounts', .5, .5, .25), ('UC-02 Manage Products', .5, .5, .25),
        ('UC-03 Manage Customer Records', .5, .5, .25), ('UC-04 Record Sale', 1, 1, .5),
        ('UC-05 Record Payment', .5, .5, .25), ('UC-06 Manage Inquiries', .5, .5, .25),
        ('UC-07 Manage Complaints', .5, .5, .25), ('UC-08 Record Feedback', .5, .25, .25)]),
    ('Lab 2 · Tenant B', [
        ('UC-09 View BI Dashboard', 1, 1, .25), ('UC-10 Generate Operational Report', .5, .5, .25),
        ('UC-11 Manage Retention Campaigns', 1, 1, .5)]),
    ('Final · Tenant C + Master', [
        ('UC-12 Manage Branches', 3, 3, 1), ('UC-13 Manage Tenants', 3, 3, 1),
        ('UC-14 Manage Subscriptions', 1.5, 1.5, .5), ('UC-15 View Platform Insights', 1.5, 1.5, .5)]),
]
POLISH = [('Fix and Polish Lab 1 and 2', 5, FD)]
WRAP = [('Final Testing', 3, QA)]


def is_work(d):
    return d not in HOLIDAYS


class Clock:
    """Working time: 8-12 and 1-5, as hours since 8:00 of a working day."""
    def __init__(self):
        self.day = START
        self.hour = 0.0

    def stamp(self, at_end=False):
        h = self.hour
        if at_end and h == 0:  # a task ending exactly at day's end shows that day's 5 PM
            d = self.day - timedelta(days=1)
            while not is_work(d):
                d -= timedelta(days=1)
            return datetime(d.year, d.month, d.day, 17)
        clock = 8 + h if h < 4 or (h == 4 and at_end) else 9 + h
        return datetime(self.day.year, self.day.month, self.day.day, int(clock), int((clock % 1) * 60))

    def add(self, days):
        hours = days * 8 + self.hour
        self.hour = 0.0
        while hours >= 8:
            hours -= 8
            self.day += timedelta(days=1)
            while not is_work(self.day):
                self.day += timedelta(days=1)
        self.hour = hours


clock = Clock()
tasks, uid = [], [0]


def task(name, level, summary=False, days=0.0, res=None, pred=None):
    uid[0] += 1
    t = dict(uid=uid[0], name=name, level=level, summary=summary, days=days, res=res, pred=pred)
    tasks.append(t)
    return t


def leaf(name, level, days, res, prev):
    t = task(name, level, days=days, res=res, pred=prev['uid'] if prev else None)
    t['start'] = clock.stamp()
    clock.add(days)
    t['finish'] = clock.stamp(at_end=True)
    return t


def close(s, kids):
    s['start'], s['finish'] = kids[0]['start'], kids[-1]['finish']
    s['days'] = sum(k['days'] for k in kids)


root = task('xpera CRM', 1, summary=True)
prev, top, leaves = None, [], []

for group, ucs in PLAN:
    if group.startswith('Final'):
        g = task('After Lab 2', 2, summary=True)
        kids = []
        for n, d, r in POLISH:
            prev = leaf(n, 3, d, r, prev)
            kids.append(prev)
        close(g, kids)
        top.append(g)
    g = task(group, 2, summary=True)
    groups = []
    for name, fd, bd, qa in ucs:
        u = task(name, 3, summary=True)
        kids = []
        for n, d, r in (('Front-End', fd, FD), ('Back-End', bd, BD), ('Quality Assurance', qa, QA)):
            prev = leaf(n, 4, d, r, prev)
            kids.append(prev)
        close(u, kids)
        groups.append(u)
    close(g, groups)
    top.append(g)

g = task('Testing', 2, summary=True)
kids = []
for n, d, r in WRAP:
    prev = leaf(n, 3, d, r, prev)
    kids.append(prev)
close(g, kids)
top.append(g)
close(root, top)

# Done on the first day, like the example chart.
# ProjectLibre labels each bar with its resource, so each one is its own resource.
for r, n in ((4, 'Hosting'), (5, 'Domain'), (6, 'Cloud Database')):
    t = task(n, 2, days=1, res=r)
    t['start'], t['finish'] = datetime(START.year, START.month, START.day, 8), datetime(START.year, START.month, START.day, 17)


def ts(d):
    return d.strftime('%Y-%m-%dT%H:%M:%S')


def dur(days):
    return f'PT{int(days * 8)}H{int((days * 8 % 1) * 60)}M0S'


def wbs_for():
    counters, out = [0] * 6, {}
    for t in tasks:
        lv = t['level']
        counters[lv - 1] += 1
        for i in range(lv, 6):
            counters[i] = 0
        out[t['uid']] = '.'.join(str(c) for c in counters[:lv])
    return out


wbs = wbs_for()
x = ['<?xml version="1.0" encoding="UTF-8" standalone="yes"?>',
     '<Project xmlns="http://schemas.microsoft.com/project">',
     '<Name>xpera CRM</Name><Title>xpera CRM</Title><ScheduleFromStart>1</ScheduleFromStart>',
     f'<StartDate>{ts(root["start"])}</StartDate><FinishDate>{ts(root["finish"])}</FinishDate>',
     '<DefaultStartTime>08:00:00</DefaultStartTime><DefaultFinishTime>17:00:00</DefaultFinishTime>',
     '<MinutesPerDay>480</MinutesPerDay><MinutesPerWeek>3360</MinutesPerWeek><DaysPerMonth>20</DaysPerMonth>',
     '<CalendarUID>1</CalendarUID>',
     '<Calendars><Calendar><UID>1</UID><Name>Standard</Name><IsBaseCalendar>1</IsBaseCalendar><WeekDays>']
for n in range(1, 8):
    if False:
        x.append(f'<WeekDay><DayType>{n}</DayType><DayWorking>0</DayWorking></WeekDay>')
    else:
        x.append(f'<WeekDay><DayType>{n}</DayType><DayWorking>1</DayWorking><WorkingTimes>'
                 '<WorkingTime><FromTime>08:00:00</FromTime><ToTime>12:00:00</ToTime></WorkingTime>'
                 '<WorkingTime><FromTime>13:00:00</FromTime><ToTime>17:00:00</ToTime></WorkingTime>'
                 '</WorkingTimes></WeekDay>')
x.append('</WeekDays><Exceptions>')
for h in sorted(HOLIDAYS):
    x.append(f'<Exception><EnteredByOccurrences>0</EnteredByOccurrences><TimePeriod><FromDate>{h}T00:00:00</FromDate>'
             f'<ToDate>{h}T23:59:00</ToDate></TimePeriod><Occurrences>1</Occurrences><Name>Holiday</Name><Type>1</Type>'
             f'<DayWorking>0</DayWorking></Exception>')
x.append('</Exceptions></Calendar></Calendars><Tasks>')
x.append('<Task><UID>0</UID><ID>0</ID><Name>xpera CRM</Name><Type>1</Type><IsNull>0</IsNull><OutlineNumber>0</OutlineNumber>'
         '<OutlineLevel>0</OutlineLevel><Summary>1</Summary></Task>')
for t in tasks:
    x.append(f'<Task><UID>{t["uid"]}</UID><ID>{t["uid"]}</ID><Name>{escape(t["name"])}</Name><Type>1</Type><IsNull>0</IsNull>'
             f'<WBS>{wbs[t["uid"]]}</WBS><OutlineNumber>{wbs[t["uid"]]}</OutlineNumber><OutlineLevel>{t["level"]}</OutlineLevel>'
             f'<Start>{ts(t["start"])}</Start><Finish>{ts(t["finish"])}</Finish><Duration>{dur(t["days"])}</Duration>'
             f'<DurationFormat>7</DurationFormat><Summary>{int(t["summary"])}</Summary><Milestone>0</Milestone><Critical>1</Critical>'
             f'<ConstraintType>0</ConstraintType><CalendarUID>-1</CalendarUID>')
    if t['pred']:
        x.append(f'<PredecessorLink><PredecessorUID>{t["pred"]}</PredecessorUID><Type>1</Type></PredecessorLink>')
    x.append('</Task>')
x.append('</Tasks><Resources>')
for u, n in RESOURCES.items():
    x.append(f'<Resource><UID>{u}</UID><ID>{u}</ID><Name>{n}</Name><Type>1</Type><MaxUnits>1.00</MaxUnits><CalendarUID>1</CalendarUID></Resource>')
x.append('</Resources><Assignments>')
a = 0
for t in tasks:
    if t['res']:
        a += 1
        x.append(f'<Assignment><UID>{a}</UID><TaskUID>{t["uid"]}</TaskUID><ResourceUID>{t["res"]}</ResourceUID><Units>1</Units>'
                 f'<Work>{dur(t["days"])}</Work><Start>{ts(t["start"])}</Start><Finish>{ts(t["finish"])}</Finish></Assignment>')
x.append('</Assignments></Project>')
out = Path(__file__).with_name('xpera-schedule.xml')
out.write_text('\n'.join(x), encoding='utf-8')

print('tasks', len(tasks), 'work tasks', sum(1 for t in tasks if t['res']))
for t in tasks:
    if t['level'] <= 2:
        print(f'{t["name"]:30} {t["days"]:5} days  {t["start"]:%b %d %Y} to {t["finish"]:%b %d %Y}')
