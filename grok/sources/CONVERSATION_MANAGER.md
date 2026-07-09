# Manager Conversation Notes (EN summary + HI original)

## Core ask
Move from Excel to centralized masters + mapped ownership so system knows **whose** data and **who is assigned**.

## Masters
1. Department Master  
2. Section Master mapped to Department  
3. Bind CM, AGM, DGM, GM, CGM (or Reporting Authority 1–5) on section  
4. Team members under section: PF → auto name/designation; role; bulk upload; add/update; **soft delete only**  
5. Admin creates sections; CM can modify own team after login  
6. One CM ↔ multiple sections supported (keep simple; don’t over-constrain)

## Project
- After dept/section → Project Master  
- One section → many projects  
- Project-wise budget allotment (allocated/fresh/spillover/total) FY-wise in project master  
- Cannot jump to budget entry before dept/section/project exist

## Budget entry
- Capital and Revenue screens  
- Monthly: show previous month reference; current blank for entry  
- Revenue: many heads; don’t show all empty — dropdown applicable heads (manager preference)  
- Store FY + month + submission date  
- Dashboard filters: Department, Section, Project, FY, Month  

## Integration / existing systems
- Refer existing portal / Project Management / BB tables (manpower, project master, budget utilization)
- May create new tables / customize; check what already exists
- Budget utilization facility “not fully there” in current portal — build so it becomes usable
- Timeline talk (manager): roughly 10–15 days style estimate — **not used as our commitment**; we phase by gates

## Immediate manager instruction
- First create Department & Section tables (even hardcoded sample) then Project then entry screens  
- Admin power for master updates later  

## Conflict with client
Manager scope is broader (hierarchy/team/dashboard/integration) than client’s “entry + DB for BI” statement. See CONFLICTS.md.
