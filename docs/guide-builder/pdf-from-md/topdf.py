import sys, re, pathlib, markdown

src = pathlib.Path(sys.argv[1]); out_html = pathlib.Path(sys.argv[2])
fonts = pathlib.Path(r"C:\Users\Nicho Lescano\Desktop\xpera-crm\code\Xpera\Fonts").as_uri()
md = src.read_text(encoding='utf-8')

# Big sections start on a new page.
body = markdown.markdown(md, extensions=['tables', 'fenced_code', 'sane_lists'])
for title in ["Every database connection", "Peer walkthrough", "How everything works", "Likely questions"]:
    body = body.replace("<h2>" + title, '<h2 class="newpage">' + title, 1)
body = body.replace("<hr />", "")

css = f"""
@font-face {{ font-family: Inter; src: url('{fonts}/Inter-Regular.ttf'); font-weight: 400; }}
@font-face {{ font-family: Inter; src: url('{fonts}/Inter-Medium.ttf'); font-weight: 500; }}
@font-face {{ font-family: Inter; src: url('{fonts}/Inter-SemiBold.ttf'); font-weight: 600; }}
@font-face {{ font-family: Inter; src: url('{fonts}/Inter-Bold.ttf'); font-weight: 700; }}
@page {{ size: A4; margin: 16mm 15mm 16mm 15mm; }}
body {{ font-family: Inter, 'Segoe UI', sans-serif; font-size: 9.6pt; line-height: 1.5; color: #121212; }}
h1 {{ font-size: 20pt; font-weight: 600; margin: 0 0 4pt; letter-spacing: -0.2pt; }}
h1 + p {{ color: #555; margin-top: 0; }}
h2 {{ font-size: 13pt; font-weight: 600; margin: 18pt 0 6pt; padding-top: 6pt; border-top: 2px solid #F43F5E; break-after: avoid; }}
h2.newpage {{ break-before: page; }}
h3 {{ font-size: 10.8pt; font-weight: 600; margin: 12pt 0 4pt; break-after: avoid; }}
p {{ margin: 4pt 0 6pt; }}
ul, ol {{ margin: 4pt 0 6pt; padding-left: 18pt; }}
li {{ margin: 1.5pt 0; }}
strong {{ font-weight: 600; }}
code {{ font-family: Consolas, monospace; font-size: 8.8pt; background: #F4F1EA; padding: 0 3px; border-radius: 3px; }}
pre {{ background: #F4F1EA; padding: 8pt 10pt; border-radius: 6px; font-size: 8.6pt; line-height: 1.35; break-inside: avoid; }}
pre code {{ background: none; padding: 0; }}
table {{ border-collapse: collapse; width: 100%; margin: 6pt 0 10pt; font-size: 8.4pt; line-height: 1.38; }}
th {{ text-align: left; font-weight: 600; background: #121212; color: #fff; padding: 4pt 5pt; }}
td {{ padding: 3.5pt 5pt; border-bottom: 1px solid #ddd; vertical-align: top; }}
tr {{ break-inside: avoid; }}
tr:nth-child(even) td {{ background: #faf8f4; }}
td code, th code {{ font-size: 7.8pt; }}
.cover {{ display: flex; align-items: center; gap: 10pt; margin-bottom: 10pt; }}
.mark {{ width: 26pt; height: 26pt; }}
.word {{ font-size: 18pt; font-weight: 600; letter-spacing: -0.3pt; }}
.tag {{ color: #F43F5E; font-weight: 600; font-size: 9pt; }}
"""

# A small hand-drawn version of the four-blade spark mark (three solid blades + one long red blade up-right).
mark = """<svg class="mark" viewBox="-10 -10 20 20"><g>
<path d="M0 0 L-7 -6 L-5.5 -7.5 Z" fill="#121212"/><path d="M0 0 L-7 6 L-5.5 7.5 Z" fill="#121212"/>
<path d="M0 0 L6 7 L7.5 5.5 Z" fill="#121212"/><path d="M0 0 L8.5 -8.5 L9.5 -7.2 Z" fill="#F43F5E"/></g></svg>"""

html = f"""<!doctype html><html><head><meta charset="utf-8"><title>xpera presentation guide</title><style>{css}</style></head>
<body><div class="cover">{mark}<div><div class="word">xpera</div><div class="tag">Level up every customer.</div></div></div>
{body}</body></html>"""
out_html.write_text(html, encoding='utf-8')
print("html ok")
