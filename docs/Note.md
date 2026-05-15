# Refactor service helpers

## 2026-05-15

- Them helper dung chung trong `Personal_Finance_Management.Service/Base`:
  - `ServiceClaimHelper`: lay `UserId`, `AdminId`, `AccountId` tu JWT claim `id`.
  - `ServiceTextHelper`: normalize required/optional text, truncate string, mask secret, mask so tai khoan, normalize enum.
- Da thay cac logic lap lai trong service layer:
  - parse claim `id` trong cac service nhu `Auth`, `User`, `Jar`, `Transaction`, `FinancialAccount`, `Dashboard`, `Onboarding`, `Goal`, `Limit`, `Notification`, `Reminder`, `AI`, `Import`, `Admin`, `Broadcast`, `Category`.
  - normalize text/enum/mask trong `AI`, `Category`, `Import`, `Reminder`, `User`, `FinancialAccount`, `ValidationServices`.
- Muc tieu: cac service tai su dung helper chung, giam private helper trung lap va giu behavior hien tai.
- Verification: `dotnet build Personal_Finance_Management/Personal_Finance_Management.sln --no-restore -m:1 -v:m` build thanh cong.
- Build con warning khong lien quan den refactor:


