# Library — מערכת ניהול ספרייה

## 1. תיאור קצר של המערכת

מערכת Web API לניהול ספרייה, בנויה ב-ASP.NET Core (.NET 8) עם ארכיטקטורה מבוססת שכבות (Core / Data / Service / API) ולקוח React לצורך בדיקת קצה-לקצה.

המערכת מאפשרת:

1. ניהול ספרים (יצירה, עדכון, מחיקה, חיפוש, סינון ו-Pagination).
2. ניהול קטגוריות.
3. השאלת ספרים.
4. החזרת ספרים.
5. צפייה בספרים זמינים.
6. צפייה בספרים שאינו זמינים כרגע.
7. הרשמה והתחברות באמצעות JWT.

## 2. הבעיה המרכזית שהמערכת פותרת

שני משתמשים יכולים לנסות להשאיל בו-זמנית את אותו ספר. אסור למערכת להגיע למצב שבו שני משתמשים הצליחו להשאיל את אותו ספר.

## 3. איך Optimistic Concurrency פותר את הבעיה

לישות `Book` יש שדה `RowVersion` (`[Timestamp] byte[]`) שממופה ל-`rowversion` ב-SQL Server ומנוהל אוטומטית על ידי EF Core.

כאשר שני משתמשים קוראים את אותו ספר ואז שניהם מנסים לעדכן אותו (לשנות אותו ל-`Loaned` ולשמור `Loan` חדש) — כל אחד מהם מחזיק גרסה (`RowVersion`) שנקראה מהמסד באותו רגע:

- העדכון הראשון שנשמר מצליח, וה-`RowVersion` בטבלה מתעדכן לערך חדש.
- העדכון השני מגיע עם `RowVersion` ישן (לא תואם למה שבמסד הנתונים כרגע), ו-EF Core זורק `DbUpdateConcurrencyException` בזמן `SaveChangesAsync`.
- ה-Service תופס את החריגה הספציפית הזו (`LoanService`) וממיר אותה לחריגת domain (`LoanConcurrencyException`), כדי שלא "לבלוע" חריגות כלליות (`catch(Exception)` אסור לפי הדרישות).

כך אין צורך לנעול טבלאות (Pessimistic Locking) — ה-DB Update השני נכשל אוטומטית כשמתגלה שהנתונים כבר השתנו, ורק ה-Request שהצליח ראשון משנה את מצב הספר.

## 4. HTTP 409 Conflict

`ExceptionHandlingMiddleware` (ב-`Library.API/Middleware`) תופס את `LoanConcurrencyException` (ואת `DbUpdateConcurrencyException` הגולמית, במידה והיא דולפת) ומחזיר:

- **Status Code:** `409 Conflict`
- **Body:**
  ```json
  {
    "statusCode": 409,
    "message": "הספר נתפס על ידי משתמש אחר. נסה שוב",
    "correlationId": "..."
  }
  ```

אירוע כזה נרשם ל-log ברמת `Warning` (לא `Error`, כי זו התנהגות עסקית צפויה ולא תקלה).

בצד ה-React (`BooksPage.jsx`), כשהשרת מחזיר `409`, מוצגת הודעה ברורה למשתמש ("הספר נתפס: ...") במקום קריסה או שגיאה גנרית.

## 5. הוראות הרצה מקומיות

### דרישות מקדימות

- .NET 8 SDK
- SQL Server (מומלץ SQL Server LocalDB, שמותקן כברירת מחדל עם Visual Studio)
- Node.js 18+ (עבור ה-React client)
- כלי `dotnet-ef` (`dotnet tool install --global dotnet-ef`)

### הגדרת Secrets (חובה — אין סודות בקוד)

מתוך `Library.API`:

```bash
dotnet user-secrets set "Jwt:Secret" "<מחרוזת ארוכה וסודית>"
```

מחרוזת ההתחברות ל-DB כבר מוגדרת ל-LocalDB ב-`appsettings.Development.json`. אם יש צורך בשרת SQL אחר, ניתן לדרוס אותה גם היא דרך `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."`.

### הרצת השרת

```bash
cd Library.API
dotnet run
```

Migrations ו-Seed Data רצים אוטומטית באתחול (`DbSeeder.SeedAsync`), כך שאין צורך בפקודות נוספות כדי שיהיו נתוני דמו.

Swagger UI זמין בכתובת: `http://localhost:5178/swagger`.

### הרצת ה-React Client

```bash
cd library.client
npm install
npm run dev
```

האפליקציה עולה על `http://localhost:5173` ומדברת עם ה-API בכתובת `http://localhost:5178/api` (מוגדר ב-`src/api.js`).

## 6. איזה Database נדרש

**SQL Server**. מחרוזת ההתחברות שבברירת מחדל (`appsettings.Development.json`) פונה ל-LocalDB בשם `mssqllocaldb`. ניתן להחליף לכל שרת SQL Server אחר על ידי שינוי/override של `ConnectionStrings:DefaultConnection`.

## 7. איך להריץ Migrations

מתיקיית שורש הפרויקט:

```bash
dotnet ef migrations add <MigrationName> --project Library.Data --startup-project Library.API
dotnet ef database update --project Library.Data --startup-project Library.API
```

הפרויקט מגיע עם שני migrations קיימים: `InitialCreate` ו-`AddConcurrency`. הרצת `dotnet run` על `Library.API` מפעילה `Database.MigrateAsync()` אוטומטית, כך שגם ללא הרצה ידנית של `dotnet ef database update` המסד יעודכן בעת עליית השרת.

## 8. משתמשי Demo להתחברות

נוצרים אוטומטית ב-Seed (רק אם טבלת Categories ריקה):

| תפקיד | Email | סיסמה |
|---|---|---|
| מנהל (IsAdmin) | `admin@library.demo` | `Admin123!` |
| מנוי רגיל | `member@library.demo` | `Member123!` |

## 9. שימוש Demo ברג'ים (Endpoints)

1. **הרשמה:** `POST /api/auth/register` עם `{ name, email, password }`.
2. **התחברות:** `POST /api/auth/login` עם `{ email, password }` — מחזיר JWT.
3. **רשימת ספרים:** `GET /api/books?pageNumber=1&pageSize=10&search=...` (Pagination מתבצע ב-Database, לא ב-Memory).
4. **השאלת ספר:** `POST /api/loans` עם `{ bookId }` (דורש `Authorization: Bearer <token>`).
5. **ההשאלות שלי:** `GET /api/loans/my-loans`.
6. **החזרת ספר:** `PUT /api/loans/{id}/return`.

כל הבדיקות הידניות ניתן לבצע גם דרך Swagger UI (`/swagger`), כולל כפתור **Authorize** להזנת ה-Bearer Token.

## 10. מבנה ארבע השכבות

```
Library.Core     Entities, DTOs, Enums, Interfaces (Repository), Security, Settings
                 → אינו תלוי בשום Project אחר.

Library.Data     AppDbContext, Fluent API Configurations, Repositories (מימוש), Migrations
                 → תלוי ב-Library.Core בלבד.

Library.Service  Services (Business Logic), AutoMapper Profile, JWT Token Generator
                 → תלוי ב-Library.Core בלבד.

Library.API      Controllers, Middleware (Exception Handling, CorrelationId), Program.cs
                 → תלוי בשלושת ה-Projects האחרים; הוא היחיד שמכיר DI ו-DbContext registration.
```

הכלל המרכזי: `Controller -> Service -> Repository -> DbContext`, כל השכבה מדברת רק עם השכבה שמתחתיה, ללא גישה ישירה ל-DbContext מתוך Controller, וללא Business Logic בתוך Repository.

## 11. בדיקות (Unit Tests)

```bash
dotnet test
```

`Library.Tests` (xUnit + Moq) כולל:

- בדיקה שהשאלת ספר מצליחה כשהספר זמין.
- בדיקה שהשאלה נדחית כשהספר כבר מושאל.
- בדיקת Concurrency אמיתית עם שני `AppDbContext` נפרדים על אותה שורת `Book`, שמוכיחה ש-`DbUpdateConcurrencyException` אכן נזרקת כשהשני מנסה לשמור אחרי שהראשון כבר עדכן את השורה.
