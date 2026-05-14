using KostinShop.Models;

namespace KostinShop.Data;

/// <summary>
/// Наполнение БД начальными данными (seed).
/// Миграции применяются вручную через "dotnet ef database update".
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Проверяет подключение к БД и при необходимости заполняет её
    /// начальными данными. Не трогает схему.
    /// </summary>
    public static void CheckConnectionAndSeed()
    {
        using var db = DbContextFactory.Create();

        if (!db.Database.CanConnect())
            throw new InvalidOperationException("Нет подключения к базе данных.");

        SeedRoles(db);
        SeedOrderStatuses(db);
        SeedCategoriesAndProducts(db);
        SeedUsersClientsOrders(db);
    }

    // ── Роли ─────────────────────────────────────────────────────────
    private static void SeedRoles(ShopDbContext db)
    {
        // FIX: проверяем каждую роль по отдельности, а не "есть хоть что-то".
        // Миграция AddAdminRole могла добавить только role_admin —
        // тогда Any()==true, но role_manager и role_logist всё равно отсутствуют.
        var existing = db.Roles.Select(r => r.Name).ToHashSet();

        var missing = new[]
        {
            new Role { Name = "role_admin",   Description = "Администратор — полный доступ, управление пользователями системы" },
            new Role { Name = "role_manager", Description = "Менеджер — управление товарами, категориями, клиентами и заказами" },
            new Role { Name = "role_logist",  Description = "Логист — просмотр и смена статусов заказов" },
        }.Where(r => !existing.Contains(r.Name)).ToList();

        if (missing.Count == 0) return;
        db.Roles.AddRange(missing);
        db.SaveChanges();
    }

    // ── Статусы заказа ────────────────────────────────────────────────
    private static void SeedOrderStatuses(ShopDbContext db)
    {
        var existing = db.OrderStatuses.Select(s => s.Name).ToHashSet();
        var missing = new[]
        {
            new OrderStatus { Name = "Новый" },
            new OrderStatus { Name = "Оформлен" },
            new OrderStatus { Name = "В обработке" },
            new OrderStatus { Name = "Отправлен" },
            new OrderStatus { Name = "Доставлен" },
            new OrderStatus { Name = "Отменён" }
        }.Where(s => !existing.Contains(s.Name)).ToList();

        if (missing.Count == 0) return;
        db.OrderStatuses.AddRange(missing);
        db.SaveChanges();
    }

    // ── Категории и товары ────────────────────────────────────────────
    private static void SeedCategoriesAndProducts(ShopDbContext db)
    {
        if (db.Categories.Any()) return;

        var cats = new[]
        {
            new Category { Name = "Смартфоны" },
            new Category { Name = "Ноутбуки" },
            new Category { Name = "Планшеты" },
            new Category { Name = "Аудиотехника" },
            new Category { Name = "Аксессуары" },
            new Category { Name = "Телевизоры" },
        };
        db.Categories.AddRange(cats);
        db.SaveChanges();

        var (phones, laptops, tablets, audio, acc, tv) =
            (cats[0], cats[1], cats[2], cats[3], cats[4], cats[5]);

        db.Products.AddRange(
            // Смартфоны
            P("Samsung Galaxy S24",          "6.2\" AMOLED, Snapdragon 8 Gen 3, 256 ГБ, 50 Мп",           79999m, phones),
            P("Apple iPhone 15",             "6.1\" Super Retina XDR, A16 Bionic, 128 ГБ",                 89990m, phones),
            P("Xiaomi Redmi Note 13 Pro",    "6.67\" AMOLED, Snapdragon 7s Gen 2, 256 ГБ",                 29999m, phones),
            P("POCO X6 Pro",                 "6.67\" AMOLED, Dimensity 8300 Ultra, 512 ГБ",                39999m, phones),
            // Ноутбуки
            P("ASUS VivoBook 15",            "15.6\" FHD, Intel Core i5-1235U, 16 ГБ RAM, 512 ГБ SSD",    54990m, laptops),
            P("Lenovo IdeaPad Slim 5",       "14\" IPS, AMD Ryzen 5 7530U, 16 ГБ RAM, 512 ГБ SSD",        49900m, laptops),
            P("Apple MacBook Air M2",        "13.6\" Liquid Retina, Apple M2, 8 ГБ, 256 ГБ SSD",         119990m, laptops),
            P("HP Pavilion 15",              "15.6\" IPS, Intel Core i7-1255U, 16 ГБ, 512 ГБ SSD",        64990m, laptops),
            // Планшеты
            P("Apple iPad 10-го поколения",  "10.9\" Liquid Retina, A14 Bionic, 64 ГБ, Wi-Fi",            52990m, tablets),
            P("Samsung Galaxy Tab S9 FE",    "10.9\" TFT, Exynos 1380, 128 ГБ, S Pen в комплекте",        39990m, tablets),
            // Аудиотехника
            P("Sony WH-1000XM5",             "Беспроводные наушники с ANC, 30 ч, Bluetooth 5.2",           27990m, audio),
            P("Apple AirPods Pro (2-е пок.)", "TWS, ANC, MagSafe зарядка, H2 чип",                        24990m, audio),
            P("JBL Charge 5",                "Портативная колонка, 20 ч, IP67, 30 Вт",                    11990m, audio),
            // Аксессуары
            P("Чехол для iPhone 15 силикон", "Защитный чехол, MagSafe совместимый, чёрный",                1490m, acc),
            P("Зарядное USB-C 65W GaN",      "GaN, совместимо с ноутбуками и смартфонами",                 2990m, acc),
            P("Кабель USB-C 2 м",            "Нейлоновая оплётка, быстрая зарядка 100W",                   990m, acc),
            P("Защитное стекло Samsung S24", "Закалённое, 9H твёрдость, полное покрытие",                  790m, acc),
            // Телевизоры
            P("LG OLED55C3RLA",              "55\" OLED, 4K, 120 Гц, HDMI 2.1, webOS, Dolby Vision",    119990m, tv),
            P("Samsung UE50CU8000",          "50\" LED, 4K, HDR10+, Tizen, Crystal Processor",            44990m, tv),
            P("Hisense 55U7KQ",              "55\" QLED, 4K, 144 Гц, Dolby Vision, Game Mode Pro",        54990m, tv)
        );
        db.SaveChanges();

        static Product P(string name, string desc, decimal price, Category cat) =>
            new() { Name = name, Description = desc, Price = price, ID_Category = cat.ID_Category };
    }

    // ── Пользователи, клиенты, заказы ────────────────────────────────
    private static void SeedUsersClientsOrders(ShopDbContext db)
    {
        if (db.Users.Any()) return;

        var roleAdmin   = db.Roles.First(r => r.Name == "role_admin");
        var roleManager = db.Roles.First(r => r.Name == "role_manager");
        var roleLogist  = db.Roles.First(r => r.Name == "role_logist");

        var statusNew  = db.OrderStatuses.First(s => s.Name == "Новый");
        var statusProc = db.OrderStatuses.First(s => s.Name == "В обработке");
        var statusSent = db.OrderStatuses.First(s => s.Name == "Отправлен");
        var statusDone = db.OrderStatuses.First(s => s.Name == "Доставлен");
        var statusCanc = db.OrderStatuses.First(s => s.Name == "Отменён");

        // ── Сотрудники ────────────────────────────────────────────────
        var admin   = CreateUser("admin",      "Admin1!",   "Администратор", null,        "Системный", "+79201000000");
        var manager = CreateUser("manager01", "Manager1!", "Алексей",   null,            "Сидоров", "+79201000001");
        var logist  = CreateUser("logist01",  "Logist1!",  "Екатерина", null,            "Волкова", "+79201000002");
        db.Users.AddRange(admin, manager, logist);
        db.SaveChanges();

        db.UserRoles.AddRange(
            new UserRole { ID_User = admin.ID_User,   ID_Role = roleAdmin.ID_Role },
            new UserRole { ID_User = manager.ID_User, ID_Role = roleManager.ID_Role },
            new UserRole { ID_User = logist.ID_User,  ID_Role = roleLogist.ID_Role }
        );
        db.SaveChanges();

        // ── Клиенты ───────────────────────────────────────────────────
        var clientData = new[]
        {
            ("Иван",    "Сергеевич",    "Козлов",     "+79202111001", "ivan_kozlov",    "Buyer1!",    90,  false),
            ("Мария",   "Андреевна",    "Новикова",   "+79202111002", "masha_nov",      "Buyer2@",   250,  false),
            ("Дмитрий", "Петрович",     "Фёдоров",    "+79202111003", "dima_fed",       "Buyer3#",     0,  false),
            ("Ольга",   "Викторовна",   "Морозова",   "+79202111004", "olga_mor",       "Buyer4$",   500,  false),
            ("Сергей",  "Иванович",     "Белов",      "+79202111005", "sergei_bel",     "Buyer5%",   120,  false),
            ("Анна",    "Дмитриевна",   "Соколова",   "+79202111006", "anna_sok",       "Buyer6^",    75,  false),
            ("Максим",  "Олегович",     "Тихонов",    "+79202111007", "max_tikh",       "Buyer7!",   330,  false),
            ("Наталья", "Юрьевна",      "Орлова",     "+79202111008", "natasha_orl",    "Buyer8@",     0,  false),
            ("Павел",   "Александрович","Крылов",     "+79202111009", "pavel_kry",      "Buyer9#",   180,  false),
            ("Виктор",  "Николаевич",   "Зайцев",     "+79202111010", "viktor_zaitsev", "DualRole1!", 410, true),
        };

        var clients     = new List<Client>();
        var clientUsers = new List<AppUser>();

        foreach (var (fn, mn, ln, phone, login, pwd, pts, isDual) in clientData)
        {
            var client = new Client
            {
                Loyalty_Points = pts,
                Registered_At  = DateTime.UtcNow.AddDays(-new Random(phone.GetHashCode()).Next(10, 400))
            };
            db.Clients.Add(client);
            db.SaveChanges();

            var user = CreateUser(login, pwd, fn, mn, ln, phone, client.ID_Client);
            db.Users.Add(user);
            db.SaveChanges();

            clients.Add(client);
            clientUsers.Add(user);
        }

        // Виктор Зайцев — дополнительно получает роль менеджера
        db.UserRoles.Add(new UserRole { ID_User = clientUsers[^1].ID_User, ID_Role = roleManager.ID_Role });
        db.SaveChanges();

        // ── Товары для заказов ────────────────────────────────────────
        var products = db.Products.ToList();
        Product Find(string fragment) => products.First(p => p.Name.Contains(fragment));

        var galaxy  = Find("Galaxy S24");
        var iphone  = Find("iPhone 15");
        var xiaomi  = Find("Redmi");
        var asus    = Find("VivoBook");
        var lenovo  = Find("IdeaPad");
        var macbook = Find("MacBook");
        var ipad    = Find("iPad");
        var tabS9   = Find("Tab S9");
        var sony    = Find("WH-1000");
        var airpods = Find("AirPods");
        var jbl     = Find("Charge 5");
        var caseIph = Find("Чехол");
        var charger = Find("Зарядное");
        var cable   = Find("Кабель");
        var glass   = Find("стекло");
        var lg      = Find("LG OLED");
        var samsung = Find("UE50");

        // ── Заказы ────────────────────────────────────────────────────
        var now = DateTime.Now;

        // Иван Козлов
        MakeOrder(db, clients[0], "г. Владимир, ул. Большая Московская, д. 12, кв. 4",  now.AddDays(-120), statusDone, (xiaomi,1),(cable,2),(glass,1));
        MakeOrder(db, clients[0], "г. Владимир, ул. Большая Московская, д. 12, кв. 4",  now.AddDays(-45),  statusDone, (charger,1),(airpods,1));
        MakeOrder(db, clients[0], "г. Владимир, ул. Большая Московская, д. 12, кв. 4",  now.AddDays(-3),   statusNew,  (galaxy,1),(caseIph,1));

        // Мария Новикова
        MakeOrder(db, clients[1], "г. Владимир, пр-т Строителей, д. 5, кв. 18",         now.AddDays(-200), statusDone, (ipad,1));
        MakeOrder(db, clients[1], "г. Владимир, пр-т Строителей, д. 5, кв. 18",         now.AddDays(-90),  statusDone, (airpods,1),(caseIph,2));
        MakeOrder(db, clients[1], "г. Владимир, пр-т Строителей, д. 5, кв. 18",         now.AddDays(-7),   statusProc, (macbook,1));

        // Дмитрий Фёдоров
        MakeOrder(db, clients[2], "г. Владимир, ул. Мира, д. 23, кв. 77",               now.AddDays(-60),  statusCanc, (lg,1));

        // Ольга Морозова
        MakeOrder(db, clients[3], "г. Владимир, ул. Ленина, д. 8, кв. 2",               now.AddDays(-300), statusDone, (asus,1),(charger,1));
        MakeOrder(db, clients[3], "г. Владимир, ул. Ленина, д. 8, кв. 2",               now.AddDays(-150), statusDone, (sony,1));
        MakeOrder(db, clients[3], "г. Владимир, ул. Ленина, д. 8, кв. 2",               now.AddDays(-30),  statusSent, (iphone,1),(cable,1));
        MakeOrder(db, clients[3], "г. Владимир, ул. Ленина, д. 8, кв. 2",               now.AddDays(-2),   statusNew,  (glass,2),(caseIph,1));

        // Сергей Белов
        MakeOrder(db, clients[4], "г. Владимир, ул. Горького, д. 41, кв. 99",           now.AddDays(-80),  statusDone, (lenovo,1));
        MakeOrder(db, clients[4], "г. Владимир, ул. Горького, д. 41, кв. 99",           now.AddDays(-14),  statusProc, (jbl,1),(cable,1));

        // Анна Соколова
        MakeOrder(db, clients[5], "г. Владимир, ул. Дворянская, д. 3, кв. 11",          now.AddDays(-50),  statusDone, (tabS9,1),(charger,1));
        MakeOrder(db, clients[5], "г. Владимир, ул. Дворянская, д. 3, кв. 11",          now.AddDays(-5),   statusNew,  (airpods,1));

        // Максим Тихонов
        MakeOrder(db, clients[6], "г. Владимир, ул. Студёная гора, д. 17, кв. 56",      now.AddDays(-180), statusDone, (macbook,1),(cable,2));
        MakeOrder(db, clients[6], "г. Владимир, ул. Студёная гора, д. 17, кв. 56",      now.AddDays(-60),  statusDone, (sony,1),(charger,1));
        MakeOrder(db, clients[6], "г. Владимир, ул. Студёная гора, д. 17, кв. 56",      now.AddDays(-1),   statusNew,  (samsung,1));

        // Наталья Орлова
        MakeOrder(db, clients[7], "г. Владимир, ул. Комсомольская, д. 6, кв. 34",       now.AddDays(-10),  statusProc, (ipad,1),(cable,1));

        // Павел Крылов
        MakeOrder(db, clients[8], "г. Владимир, ул. Вокзальная, д. 2, кв. 1",           now.AddDays(-70),  statusDone, (asus,1),(airpods,1));
        MakeOrder(db, clients[8], "г. Владимир, ул. Вокзальная, д. 2, кв. 1",           now.AddDays(-20),  statusSent, (xiaomi,1),(glass,1));

        // Виктор Зайцев (dual-role)
        MakeOrder(db, clients[9], "г. Владимир, Октябрьский пр-т, д. 31, кв. 88",       now.AddDays(-250), statusDone, (iphone,1),(charger,1),(cable,1));
        MakeOrder(db, clients[9], "г. Владимир, Октябрьский пр-т, д. 31, кв. 88",       now.AddDays(-90),  statusDone, (sony,1),(airpods,1));
        MakeOrder(db, clients[9], "г. Владимир, Октябрьский пр-т, д. 31, кв. 88",       now.AddDays(-8),   statusProc, (lg,1));
    }

    // ── Хелперы ───────────────────────────────────────────────────────
    private static AppUser CreateUser(
        string login, string password,
        string firstName, string? middleName, string lastName,
        string phone, int? clientId = null) => new()
    {
        Login        = login,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        First_Name   = firstName,
        Middle_Name  = middleName,
        Last_Name    = lastName,
        Phone        = phone,
        ID_Client    = clientId
    };

    private static void MakeOrder(
        ShopDbContext db, Client client, string address,
        DateTime date, OrderStatus status,
        params (Product Product, int Qty)[] items)
    {
        var order = new Order
        {
            ID_Client        = client.ID_Client,
            Delivery_Address = address,
            Order_date       = date,
            ID_Order_Status  = status.ID_Order_Status
        };
        db.Orders.Add(order);
        db.SaveChanges();

        foreach (var (product, qty) in items)
            db.ProductOrders.Add(new ProductOrder
            {
                ID_Order       = order.ID_Order,
                ID_Product     = product.ID_Product,
                Quantity       = qty,
                Price_at_order = product.Price
            });

        db.SaveChanges();
    }
}
