# Renders excerpts of the real source files as pictures (Visual Studio light colors, real line numbers).
# usage: python codepics.py <code folder> <out folder>
import sys, os, json
from pygments.lexers import CSharpLexer, JsonLexer
from pygments.token import Token
from PIL import Image, ImageDraw, ImageFont

CODE, OUT = sys.argv[1], sys.argv[2]
os.makedirs(OUT, exist_ok=True)

# id, file, line ranges
SNIPS = [
    ('company',      'Xpera.domain/entities/Company.cs',              [(7, 19)]),
    ('companydb',    'Xpera.domain/entities/CompanyDatabase.cs',      [(7, 21)]),
    ('sale',         'Xpera.domain/entities/Sale.cs',                 [(1, 16)]),
    ('mastercontext','Xpera.infrastructure/data/MasterCrmDbContext.cs',[(10, 58)]),
    ('tenantcontext','Xpera.infrastructure/data/TenantCrmDbContext.cs',[(9, 38)]),
    ('tenantsale',   'Xpera.infrastructure/data/TenantCrmDbContext.cs',[(126, 172)]),
    ('migration',    'Xpera.infrastructure/Migrations/Tenant/20261111094514_InitialTenantCrm.cs', [(174, 205)]),
    ('view',         'Xpera.infrastructure/Migrations/Tenant/20261111094514_InitialTenantCrm.cs', [(324, 331)]),
    ('appsettings',  'Xpera.api/appsettings.json',                    [(1, 99)]),
    ('apiprogram',   'Xpera.api/Program.cs',                          [(1, 16)]),
    ('db',           'Xpera.infrastructure/data/Db.cs',               [(38, 70)]),
    ('dbcontexts',   'Xpera.infrastructure/data/DbContexts.cs',       [(7, 21)]),
    ('login',        'Xpera/LoginForm.cs',                            [(77, 113), (126, 131)]),
    ('finddstore',   'Xpera.infrastructure/data/MasterDb.cs',         [(11, 45)]),
    ('session',      'Xpera.infrastructure/data/StoreDb.cs',          [(9, 23)]),
    ('signin',       'Xpera.infrastructure/data/StoreDb.cs',          [(48, 54), (68, 91)]),
    ('salesbutton',  'Xpera/Screens/SalesScreen.cs',                  [(301, 328)]),
    ('savesale',     'Xpera.infrastructure/data/StoreDb.cs',          [(230, 260)]),
    ('readview',     'Xpera.infrastructure/data/StoreDb.cs',          [(166, 172), (278, 283)]),
    ('controllerbase','Xpera.api/Controllers/StoreControllerBase.cs', [(13, 47)]),
    ('payments',     'Xpera.api/Controllers/PaymentsController.cs',   [(7, 22)]),
]

# Visual Studio (light) colors
COLORS = {
    Token.Keyword: '#0000FF', Token.Keyword.Type: '#0000FF', Token.Name.Builtin: '#0000FF',
    Token.Comment: '#008000', Token.String: '#A31515', Token.Literal.String: '#A31515',
    Token.Name.Class: '#2B91AF', Token.Number: '#098658', Token.Literal.Number: '#098658',
    Token.Name.Tag: '#2E75B6', Token.Name.Attribute: '#2E75B6',
}
TYPES = {'Company', 'CompanyDatabase', 'Sale', 'SaleItem', 'Customer', 'User', 'Payment', 'Plan', 'Subscription',
         'DbSet', 'DbContext', 'IdentityDbContext', 'MasterCrmDbContext', 'TenantCrmDbContext', 'ModelBuilder',
         'DbContextOptions', 'DbContextOptionsBuilder', 'StoreInfo', 'SignInResult', 'Session', 'StoreDb', 'MasterDb',
         'Db', 'DbContexts', 'SqlConnection', 'SqlException', 'SqlConnectionStringBuilder', 'Dictionary', 'List',
         'DateTime', 'String', 'StringComparison', 'DeleteBehavior', 'Notice', 'NoticeKind', 'MainForm', 'EventArgs',
         'DataGridViewRow', 'Convert', 'Money', 'CustomerSummaryRow', 'CustomerBalanceRow', 'WebApplication',
         'IActionResult', 'ControllerBase', 'StoreControllerBase', 'PaymentsController', 'StoreRecords', 'Func',
         'ArgumentException', 'ReferentialAction', 'Microsoft', 'ThreadStatic', 'ApiController', 'Route', 'HttpGet',
         'HttpPost', 'IdentityUser', 'Request'}
DEFAULT = '#1E1E1E'

def color(tok, text):
    if tok in Token.Name and text in TYPES: return '#2B91AF'
    while tok is not None:
        if tok in COLORS: return COLORS[tok]
        tok = tok.parent
    return DEFAULT

S = 2  # draw at 2x for sharp print
font = ImageFont.truetype('C:/Windows/Fonts/consola.ttf', 15 * S)
bold = ImageFont.truetype('C:/Windows/Fonts/consolab.ttf', 14 * S)
ui = ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf', 14 * S)
CW = font.getbbox('M')[2]
LH = 21 * S
WRAP = 100
sizes = {}

for sid, rel, ranges in SNIPS:
    path = os.path.join(CODE, rel)
    lines = open(path, encoding='utf-8-sig').read().replace('\t', '    ').split('\n')
    lexer = JsonLexer() if rel.endswith('.json') else CSharpLexer()
    rows = []  # (number or None, [(color, text)]) ; None number = continuation, 'gap' = ⋮
    for ri, (a, b) in enumerate(ranges):
        b = min(b, len(lines))
        while b >= a and lines[b - 1].strip() == '': b -= 1
        if ri: rows.append(('gap', []))
        chunk = lines[a - 1:b]
        # remove the common indent so the picture is not mostly empty space
        ind = min((len(l) - len(l.lstrip()) for l in chunk if l.strip()), default=0)
        for n, line in zip(range(a, b + 1), chunk):
            line = line[ind:]
            toks = [(color(t, v), v) for t, v in lexer.get_tokens(line) if v != '\n']
            # soft-wrap long lines
            cur, width, first = [], 0, True
            for c, v in toks:
                while width + len(v) > WRAP:
                    cut = WRAP - width
                    soft = max(v.rfind(ch, 0, cut) for ch in ' ;,(')
                    if soft > 0: cut = soft + 1                     # break after a space ; , or (
                    elif width > 40: cut = 0                        # move the whole word to the next line
                    if cut: cur.append((c, v[:cut]))
                    rows.append((n if first else None, cur)); first = False
                    v = v[cut:].lstrip(' '); cur, width = [(DEFAULT, '        ')], 8
                cur.append((c, v)); width += len(v)
            rows.append((n if first else None, cur))
    longest = max(sum(len(v) for _, v in r) for _, r in rows)
    numw = 5 * CW
    pad = 14 * S
    bar = 30 * S
    w = max(pad + numw + longest * CW + pad, 360 * S)
    h = bar + pad + LH * len(rows) + pad
    img = Image.new('RGB', (w, h), '#FFFFFF')
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, w - 1, bar], fill='#EEEEF2')
    d.text((pad, bar // 2), rel.replace('/', ' / '), font=ui, fill='#1E1E1E', anchor='lm')
    d.rectangle([0, bar, pad + numw - 6 * S, h], fill='#F7F7F7')
    y = bar + pad
    for num, toks in rows:
        if num == 'gap':
            d.text((pad + numw - 3 * CW, y), '⋮', font=font, fill='#999999')
            d.line([pad + numw, y + LH // 2, w - pad, y + LH // 2], fill='#DDDDDD', width=S)
        else:
            if num: d.text((pad + numw - 2 * CW, y), str(num).rjust(4)[-4:], font=font, fill='#2B91AF', anchor='ra')
            x = pad + numw
            for c, v in toks:
                d.text((x, y), v, font=font, fill=c); x += len(v) * CW
        y += LH
    d.rectangle([0, 0, w - 1, h - 1], outline='#C8C8C8', width=S)
    img.save(os.path.join(OUT, sid + '.png'))
    sizes[sid] = [w, h]
    print(sid, w, h)

json.dump(sizes, open(os.path.join(OUT, 'sizes.json'), 'w'))
