# xpera — design evaluation prep

The design evaluation checks whether the screens are clear, consistent, and possible to build before coding starts.

## 1. What evaluators usually look at

| Area | What they ask | Where xpera stands |
|---|---|---|
| Consistency | Same layout, colors, buttons, and fonts on every screen? | Yes. Same sidebar, header, cards, and two brand colors on all 9 screens. |
| Navigation | Can a user find every feature fast? | Sidebar with 7 items; each role only sees its own menus. |
| Matches the use cases | Does every use case have a screen? | Yes. UC-01 to UC-09 each map to a screen (see section 3). |
| Feedback | Does the user know an action worked or failed? | **Gap:** add success messages, error messages, and confirmation pop-ups. |
| Readability | Is text easy to read? Enough contrast? | Mostly yes. **Watch out:** red text on light backgrounds can be hard to read at small sizes. Use red mainly for buttons, badges, and big numbers. |
| Can it be built? | Can WinForms really do this design? | Yes, with care. See section 4. |

## 2. Missing screens and states to add before the evaluation
1. **Add/Edit Customer form**
2. **Add/Edit User form** (owner only)
3. **Log Inquiry form**
4. **Confirmation pop-ups**, like "Delete this customer?" and "Save this credit sale?"
5. **Success and error messages**, like "Sale saved" and "Wrong username or password (2 tries left)"
6. **Empty states**, for example the Inquiries board with no inquiries yet
7. **Account locked** message on the login screen

Ask Claude to design these in Figma in the same style.

## 3. Screen ↔ use case map (show this in the presentation)

| Use case | Screen |
|---|---|
| UC-01 Log In | Login |
| UC-02 Manage User Accounts | Users |
| UC-03 Manage Customer Records | Customers + Customer profile |
| UC-04 Record Sale | Record sale |
| UC-05 Record Credit Payment | Payments |
| UC-06 View Overdue Accounts | Payments (Overdue filter) + Dashboard |
| UC-07 Log Inquiry or Complaint | Inquiries (+ Log inquiry button) |
| UC-08 Update Inquiry Status | Inquiries board |
| UC-09 Generate Reports | Reports |

## 4. Building it in WinForms (the "is it possible?" question)
- **Rounded corners and modern controls:** default WinForms looks old-fashioned. A free library like **Guna UI2 (WinForms)** gives rounded buttons, cards, and text boxes. Otherwise, draw rounded panels yourself.
- **Fonts:** install Sora and Inter (free from Google Fonts) on the computer that runs the app. If they're missing, use Segoe UI.
- **Colors:** set them once in a `Theme` class (Signal Red, Carbon, Bone) so every form uses the same values.
- **Sidebar:** one main form with a sidebar on the left. Each page is a UserControl loaded into the right side.
- **Charts (Reports):** use the built-in Chart control, or draw simple bars.

## 5. Likely evaluator questions and short answers
- **"Why red and black?"** Red is the customer and growth: energy and the gaming spirit. Carbon is the store: strong and reliable, like a premium PC case.
- **"Why a desktop app?"** Stores run it on the counter PC. It works without internet, and it's what WinForms is built for.
- **"Why no inventory?"** xpera is a CRM. Its job is customers, not stock. Inventory belongs to an ERP.
- **"How do roles work?"** At login, the system checks the user's role and only shows that role's menu items.
- **"What if a customer doesn't pay?"** The account turns **Overdue** in red, shows up on the Dashboard and the Payments page, and staff can call them.

## 6. Checklist for evaluation day
- [ ] Figma open on the **CRM Screens** page (zoom out to show all 9, then walk through the flow)
- [ ] Brand Guide page ready (logo meaning + colors)
- [ ] Paper printed (Rationale + Use Case)
- [ ] Walk through one full flow out loud: **Login → Dashboard → Customer profile → Record sale (credit) → Payments**
- [ ] Missing screens from section 2 added
- [ ] Ask the teacher about the Company A/B/C requirement if it's still unclear
