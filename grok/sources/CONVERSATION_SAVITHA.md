# Savitha Conversation Notes

## Tables
- Expect multiple tables (not one mega-table): Project Master related tables, budget utilization, manpower patterns from existing Project Management
- Creating own DB/tables is OK; proceed and adjust later
- Employee/hierarchy columns till CM can work with department + section; after CM → team + projects

## Relationships
- CGM → … → CM chain
- CM has many resources; resources handle projects
- Map via project code / department id
- One CM → multiple sections (1:many common); many-to-many edge cases later
- One person → multiple projects common

## First UI (as explained)
- On land (as CM): show hierarchy reporting chain (CGM…CM), department, section
- Seniors above = read-only (admin-maintained)
- Juniors/team/projects under CM = CM can add/edit
- Admin enters from top until CM level

## Flow to budget
- Dept → Section → Projects  
- Then choose Capital or Revenue for that context  
- Discussion conflict on whether Revenue is project-level or section-level — later aligned that **Revenue is section-only**; Capital is project-level  
- Also discussed whether Capital/Revenue appears after section vs after project — see CONFLICT-06

## Practical advice from Savitha
- Start with schema; show Sir; doubts clear after starting  
- Print/explain mapping to Sir  
- Confusing until something concrete exists — but **our user rule overrides**: plan first, no code without permission
