F = 'font-family="Segoe UI, Inter, Arial, sans-serif"'
W, H = 1100, 1050
uc = [  # (number, name, y)
    (1, "Manage Staff Accounts", 132), (2, "Manage Products", 184), (3, "Manage Customer Records", 236),
    (4, "Record Sale", 288), (5, "Record Payment", 340), (6, "Manage Inquiries", 392),
    (7, "Manage Complaints", 444), (8, "Record Feedback", 496),
    (9, "View BI Dashboard", 586), (10, "Generate Operational Report", 638),
    (11, "Manage Retention Campaigns", 690),
    (12, "Manage Branches", 780),
    (13, "Manage Tenants", 870), (14, "Manage Subscriptions", 922), (15, "View Platform Insights", 974),
]
Y = {n: y for n, _, y in uc}
actors = {  # name: (x, y, side, use cases)
    "Cashier": (110, 288, "L", [3, 4, 5]),
    "Staff": (110, 470, "L", [2, 3, 6, 7, 8, 11]),
    "Admin": (990, 480, "R", [1, 2, 9, 10, 11, 12]),
    "Super Admin": (990, 922, "R", [13, 14, 15]),
}
o = [f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}"><rect width="{W}" height="{H}" fill="#FFFFFF"/>',
     f'<rect x="270" y="20" width="560" height="{H - 50}" rx="14" fill="#F4F1EA" stroke="#121212" stroke-width="2"/>',
     f'<text x="550" y="56" {F} font-size="22" font-weight="700" text-anchor="middle" fill="#121212">xpera CRM</text>']
for y in (530, 724, 814):
    o.append(f'<line x1="286" y1="{y}" x2="814" y2="{y}" stroke="#B9B3A5" stroke-width="1.2" stroke-dasharray="6 4"/>')
for y, label, col in ((100, "Tenant A, B, C · Main transactions and data collection", "#6B675E"),
                      (554, "Tenant B, C · Business intelligence and actions", "#6B675E"),
                      (748, "Tenant C · Branching", "#6B675E"), (838, "Master · Super Admin", "#C81E45")):
    o.append(f'<text x="290" y="{y}" {F} font-size="14" font-weight="600" fill="{col}">{label}</text>')
for name, (x, y, side, ucs) in actors.items():
    ax, ex = (x + 22, 360.0) if side == "L" else (x - 22, 740.0)
    for n in ucs:
        o.append(f'<line x1="{ax}" y1="{y}" x2="{ex}" y2="{Y[n]}" stroke="#8F897C" stroke-width="1.4"/>')
for n, name, y in uc:
    o.append(f'<ellipse cx="550" cy="{y}" rx="190.0" ry="21.0" fill="#FFFFFF" stroke="#121212" stroke-width="1.5"/>')
    o.append(f'<text x="550" y="{y + 6}" {F} font-size="17" text-anchor="middle" fill="#121212"><tspan font-weight="700" fill="#F43F5E">UC-{n:02d}</tspan>  {name}</text>')
for name, (x, y, side, ucs) in actors.items():
    o.append(f'<circle cx="{x}" cy="{y - 22}" r="10" fill="none" stroke="#121212" stroke-width="2.2"/><path d="M{x} {y - 12}V{y + 20}M{x - 18} {y}H{x + 18}M{x} {y + 20}L{x - 14} {y + 46}M{x} {y + 20}L{x + 14} {y + 46}" stroke="#121212" stroke-width="2.2" fill="none"/>')
    o.append(f'<text x="{x}" y="{y + 72}" {F} font-size="17" font-weight="700" text-anchor="middle" fill="#121212">{name}</text>')
    if name == "Admin":
        o.append(f'<text x="{x}" y="{y + 92}" {F} font-size="14" text-anchor="middle" fill="#6B675E">(Store Owner)</text>')
o.append('</svg>')
open(__file__.replace('.py', '.svg'), 'w', encoding='utf-8').write('\n'.join(o))
