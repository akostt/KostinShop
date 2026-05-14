using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KostinShop.Migrations
{
    /// <summary>
    /// Приводит базу в финальное состояние:
    /// 1. Переименовывает FK_AppUser_Client → FK_AppUser_Client_ID_Client
    /// 2. Пересоздаёт IX_AppUser_ID_Client как фильтрованный (WHERE ID_Client IS NOT NULL)
    /// 3. Добавляет базовые роли (role_admin, role_manager, role_logist)
    /// Все операции идемпотентны (IF EXISTS / IF NOT EXISTS).
    /// </summary>
    public partial class FinalFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Переименование FK ─────────────────────────────────────
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_AppUser_Client'
                      AND parent_object_id = OBJECT_ID('AppUser')
                )
                BEGIN
                    ALTER TABLE [AppUser] DROP CONSTRAINT [FK_AppUser_Client]
                END

                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE parent_object_id = OBJECT_ID('AppUser')
                      AND referenced_object_id = OBJECT_ID('Client')
                )
                BEGIN
                    ALTER TABLE [AppUser]
                        ADD CONSTRAINT [FK_AppUser_Client_ID_Client]
                        FOREIGN KEY ([ID_Client])
                        REFERENCES [Client] ([ID_Client])
                        ON DELETE SET NULL
                END
            ");

            // ── 2. Пересоздание индекса как фильтрованного ───────────────
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_AppUser_ID_Client'
                      AND object_id = OBJECT_ID('AppUser')
                      AND has_filter = 0
                )
                BEGIN
                    DROP INDEX [IX_AppUser_ID_Client] ON [AppUser]
                END

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_AppUser_ID_Client'
                      AND object_id = OBJECT_ID('AppUser')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_AppUser_ID_Client]
                    ON [AppUser] ([ID_Client])
                    WHERE [ID_Client] IS NOT NULL
                END
            ");

            // ── 3. Базовые роли ──────────────────────────────────────────
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Role] WHERE [Name] = 'role_manager')
                    INSERT INTO [Role] ([Name], [Description])
                    VALUES ('role_manager', 'Менеджер — управление товарами, категориями, клиентами, заказами')

                IF NOT EXISTS (SELECT 1 FROM [Role] WHERE [Name] = 'role_logist')
                    INSERT INTO [Role] ([Name], [Description])
                    VALUES ('role_logist', 'Логист — просмотр и смена статусов заказов')

                IF NOT EXISTS (SELECT 1 FROM [Role] WHERE [Name] = 'role_admin')
                    INSERT INTO [Role] ([Name], [Description])
                    VALUES ('role_admin', 'Администратор — полный доступ, управление пользователями системы')
            ");

            // ── 4. Базовые статусы заказов ───────────────────────────────
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Order_Status] WHERE [Name] = 'Новый')
                    INSERT INTO [Order_Status] ([Name]) VALUES ('Новый')
                IF NOT EXISTS (SELECT 1 FROM [Order_Status] WHERE [Name] = 'В обработке')
                    INSERT INTO [Order_Status] ([Name]) VALUES ('В обработке')
                IF NOT EXISTS (SELECT 1 FROM [Order_Status] WHERE [Name] = 'Отправлен')
                    INSERT INTO [Order_Status] ([Name]) VALUES ('Отправлен')
                IF NOT EXISTS (SELECT 1 FROM [Order_Status] WHERE [Name] = 'Доставлен')
                    INSERT INTO [Order_Status] ([Name]) VALUES ('Доставлен')
                IF NOT EXISTS (SELECT 1 FROM [Order_Status] WHERE [Name] = 'Отменён')
                    INSERT INTO [Order_Status] ([Name]) VALUES ('Отменён')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_AppUser_Client_ID_Client'
                      AND parent_object_id = OBJECT_ID('AppUser')
                )
                BEGIN
                    ALTER TABLE [AppUser] DROP CONSTRAINT [FK_AppUser_Client_ID_Client]
                END

                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE parent_object_id = OBJECT_ID('AppUser')
                      AND referenced_object_id = OBJECT_ID('Client')
                )
                BEGIN
                    ALTER TABLE [AppUser]
                        ADD CONSTRAINT [FK_AppUser_Client]
                        FOREIGN KEY ([ID_Client])
                        REFERENCES [Client] ([ID_Client])
                        ON DELETE SET NULL
                END
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS [IX_AppUser_ID_Client] ON [AppUser]
                CREATE UNIQUE INDEX [IX_AppUser_ID_Client] ON [AppUser] ([ID_Client])
            ");
        }
    }
}
