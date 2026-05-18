# BookTracker Bruno collection

API requests for [Bruno](https://www.usebruno.com/) (open-source Postman alternative, git-friendly).

## Usage

1. Install Bruno: `brew install --cask bruno` (or download from the site).
2. Open Bruno → **Open Collection** → pick this folder (`backend/bruno`).
3. Top-right environment selector → choose **Local**.
4. Run **Auth → Login (demo)**. Token is captured into the `token` env var automatically.
5. Run any **Books** request. Bearer header is set from `{{token}}`.

## Flow suggestions

- **First time:** Login (demo) → List → Create → Update → Delete.
- **Brand new user:** Auth → Register (random email) → List → Create → ...

## Environment vars

| Var | Default | Purpose |
| --- | --- | --- |
| `baseUrl` | `http://localhost:5184` | API host |
| `apiVersion` | `v1` | URL version segment |
| `demoEmail` / `demoPassword` | `demo@demo.com` / `Demo@123` | Seeded demo user |
| `token` | (captured) | JWT from login/register |
| `lastBookId` | (captured) | ID of last created book, used by Update/Delete |
| `bookId` | `22222222-...221` | Seeded demo book for `Get` |
