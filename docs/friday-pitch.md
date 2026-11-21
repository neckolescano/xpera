# Friday pitch: xpera (Rationale + Use Case)

About 3 minutes. Speak to the class, not the paper. **Bold** = say it with emphasis. (Brackets) = what to do.

---

## 1. Opening (15 sec)
"Good morning, Sir. I'm Nicho, and this is **xpera**.
We didn't misspell 'experience.' We just gave it an upgrade."

(Pause for the reaction.)

## 2. The problem (30 sec)
"Gaming stores, the ones that sell graphics cards, processors, and full PC builds, live on **loyal customers**. Gamers keep coming back for upgrades and new releases.

But most of these stores still track customers in **notebooks and chat threads**. Inquiries get lost, complaints get forgotten, and the owner can't tell who's staying and who's slowly leaving."

## 3. The company and the name (30 sec)
"So I started **Xpera**, a company that builds a CRM made only for gaming stores.

The name joins **XP**, the experience points gamers earn to level up, with **era**. It's the era where stores level up by **truly knowing their customers**. Our tagline: **Level up every customer.**"

## 4. How it works: tenants (45 sec)
"xpera works like **Gmail for gaming stores**. It's one system, but every store has its **own private inbox**, which here is its own database.

Each store that subscribes is called a **tenant**, and its plan grows with the store:
- **Tenant A** is for small stores. It collects sales, payments, inquiries, complaints, and feedback.
- **Tenant B** adds business intelligence: a dashboard with KPIs and **six graphs**, printable reports, and retention actions to bring customers back.
- **Tenant C** adds **branches**, for stores with several locations.

And above all of them is the **Master**, where I, as the **Super Admin**, manage every store and its subscription."

## 5. Use case (45 sec)
(Point to the diagram.)

"There are **four users**:
- The **Super Admin**, that's me, manages tenants, subscriptions, and platform insights.
- The **Admin**, the store owner, manages staff and products, views the dashboard and reports, runs and **tracks retention campaigns**, and handles branches.
- The **Cashier** handles customers and payments, and records sales at a **simple checkout**, where rewards are applied.
- The **Staff** updates **product availability**, handles inquiries, complaints, and feedback, and **tells customers about campaigns**.

A **retention campaign** is a promo or loyalty reward with a **start and end date**. While it runs, xpera tracks the **days left**, who was reached, who **came back** or used the reward, and **how much they spent**, so the owner knows if it worked."

Everything sits inside **one box, because it's one system**. The users stand outside it. The sections inside show which plan unlocks each feature, so a Tenant A store simply never sees the dashboard."

## 6. Tools and closing (20 sec)
"I'll build it in **C# Windows Forms** in Visual Studio 2026, with **SQL Server** managed in SSMS 22, planned in **ProjectLibre**, and tested with at least **200 transactions**.

Other stores just sell PC parts. With xpera, they sell an **experience**, and their customers keep coming back to level up. Thank you."

---

## Likely questions (short answers)

**"What's a tenant?"**
A store that subscribes to xpera. Each one has its own private database, so stores never see each other's customers.

**"Why three plans? Why not give everyone Tenant C?"**
Not every store needs or wants to pay for everything. A small shop wants something simple and cheap. As it grows, it upgrades, and the features unlock without switching systems.

**"Do you build A, B, and C separately?"**
No. I build the features once. The store's plan (A, B, or C) decides which menus it can open.

**"What is branching?"**
One owner with several stores, like RigCity Cebu and RigCity Davao. They share one database, and every record is labeled by branch, so the owner can compare them.

**"What's the difference between Admin and Super Admin?"**
The Admin runs one store. The Super Admin runs the whole xpera platform: all stores and their subscriptions.

**"What are the six graphs?"**
Monthly sales, new vs. returning customers, top customers, concerns by type, customer satisfaction, and retention status.

**"How do you track a promo or loyalty reward?"**
Every campaign has a name, type (promo or loyalty), target customers, and a start and end date. Its status shows Upcoming, Active, or Ended, with the days left. Staff mark each customer as reached, came back, or used the reward, and every sale during the campaign is linked to it. The admin sees the results: reached, came back, redeemed, and sales earned.

**"How does the customer get the promo? Who gives it?"**
Admin creates it → Staff tells the customer (call, text, or in person; xpera doesn't send SMS automatically) → the customer comes back → the Cashier picks the customer at checkout, xpera shows the promo, and the Cashier applies it. That sale is linked to the campaign, so the Admin can track it.

**"Why was the campaign made? Where can I see the reason, and who sees it?"**
Every campaign saves its **reason**, the **dashboard graph it came from**, and a **goal**. Example: "At-risk customers went up from 31 to 47 (Graph 6), so the goal is to bring back 15 by Oct 31." The Admin sees it on the Retention screen and in the printable Campaign report. The Staff sees it too, so they can explain the promo when they call. The Cashier only sees the reward at checkout.

**"Do you have inventory or a POS?"**
No full inventory. Products only show Available or Not available, so staff can answer quickly and follow up with customers who are waiting. Record Sale works like a small POS, but every sale goes under a customer's name.

**"Why is tracking not a separate use case?"**
Manage Retention Campaigns covers creating the campaign, following up with customers, and tracking the results, because they all happen on one screen. The same goes for Manage Branches, which includes comparing branches.

**"Strategic vs. operational?"**
KPIs on the dashboard are **strategic**: the big picture for decisions. Reports are **operational**: the day-to-day details, which can be printed or saved as PDF.

**"Why only gaming stores?"**
A focused CRM fits the real work, like warranty claims, installments, and upgrades. General CRMs miss that.

## Tips
- Practice it out loud **3 times** with a timer.
- Keep the paper in your hand, but **look at the class**.
- If you forget a line, go back to the one idea that matters: **"one system, many stores, each with its own data."**
