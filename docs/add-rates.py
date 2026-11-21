# Adds Davao-based rates to a ProjectLibre schedule saved as XML (File > Save As > MS Project XML).
# Usage:  python add-rates.py <input.xml> <output.xml>
# Then open the output in ProjectLibre and Save As .pod. Costs show up in the Cost column and in
# Resource Usage. Change the numbers below to try other rates.
import re
import sys

# People: pesos per hour. Based on typical junior IT pay in Davao City (around ₱19,000 to ₱27,000
# a month for 22 days × 8 hours). Overtime is 125% of the hourly rate, as the Labor Code requires
# on ordinary working days.
PEOPLE = {
    'Nicho FD': 130,   # junior front-end developer, about ₱22,900 a month
    'Nicho BD': 150,   # junior back-end / database developer, about ₱26,400 a month
    'Nicho QA': 110,   # junior QA tester, about ₱19,400 a month
}
# Things you pay for once, not by the hour (ProjectLibre's "Cost per use").
ONE_TIME = {
    'Hosting': 1500,         # small cloud/VPS hosting for about 3 months
    'Domain': 800,           # one .com domain for a year
    'Cloud Database': 1800,  # small managed SQL Server database for about 3 months
}


def add_fields(block, fields):
    # Put the rate fields right after the resource's <StandardRateFormat>, replacing any old ones.
    for tag in ('StandardRate', 'OvertimeRate', 'CostPerUse'):
        block = re.sub(rf'\s*<{tag}>[^<]*</{tag}>', '', block)
    block = re.sub(r'<StandardRateFormat>\d+</StandardRateFormat>', '<StandardRateFormat>2</StandardRateFormat>', block)
    block = re.sub(r'<OvertimeRateFormat>\d+</OvertimeRateFormat>', '<OvertimeRateFormat>2</OvertimeRateFormat>', block)
    # The Microsoft Project XML order is StandardRate, StandardRateFormat, OvertimeRate, OvertimeRateFormat, CostPerUse.
    block = block.replace('<StandardRateFormat>2</StandardRateFormat>',
                          f'<StandardRate>{fields["StandardRate"]}</StandardRate><StandardRateFormat>2</StandardRateFormat>'
                          f'<OvertimeRate>{fields["OvertimeRate"]}</OvertimeRate>', 1)
    return block.replace('<OvertimeRateFormat>2</OvertimeRateFormat>',
                         f'<OvertimeRateFormat>2</OvertimeRateFormat><CostPerUse>{fields["CostPerUse"]}</CostPerUse>', 1)


def main(src, dst):
    s = open(src, encoding='utf-8').read()
    s = re.sub(r'<CurrencySymbol>[^<]*</CurrencySymbol>', '<CurrencySymbol>₱</CurrencySymbol>', s, count=1)
    if '<CurrencyCode>' not in s:
        s = s.replace('<CurrencySymbol>₱</CurrencySymbol>', '<CurrencySymbol>₱</CurrencySymbol><CurrencyCode>PHP</CurrencyCode>', 1)

    a, b = s.index('<Resources>'), s.index('</Resources>')
    resources = s[a:b]

    def fix(match):
        block = match.group(0)
        name = re.search(r'<Name>([^<]*)</Name>', block).group(1)
        if name in PEOPLE:
            rate = PEOPLE[name]
            return add_fields(block, {'StandardRate': rate, 'OvertimeRate': round(rate * 1.25, 2), 'CostPerUse': 0})
        if name in ONE_TIME:
            return add_fields(block, {'StandardRate': 0, 'OvertimeRate': 0, 'CostPerUse': ONE_TIME[name]})
        return block

    resources = re.sub(r'<Resource>.*?</Resource>', fix, resources, flags=re.S)
    s = s[:a] + resources + s[b:]
    open(dst, 'w', encoding='utf-8').write(s)

    found = re.findall(r'<Name>([^<]*)</Name>\s*(?:<[^>]+>[^<]*</[^>]+>\s*)*?<StandardRate>([^<]*)</StandardRate>', resources)
    print('rates added:', ', '.join(f'{n} ₱{r}/h' for n, r in found if n in PEOPLE),
          '| one-time:', ', '.join(f'{n} ₱{v}' for n, v in ONE_TIME.items()))


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2])
