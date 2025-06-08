﻿CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                                                       "MigrationId" character varying(150) NOT NULL,
                                                       "ProductVersion" character varying(32) NOT NULL,
                                                       CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "AspNetRoles" (
                               "Id" text NOT NULL,
                               "Name" character varying(256),
                               "NormalizedName" character varying(256),
                               "ConcurrencyStamp" text,
                               CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetUsers" (
                               "Id" text NOT NULL,
                               "DisplayName" character varying(255) NOT NULL,
                               "PasswordLastChanged" timestamp with time zone NOT NULL,
                               "UserName" character varying(256),
                               "NormalizedUserName" character varying(256),
                               "Email" character varying(256),
                               "NormalizedEmail" character varying(256),
                               "EmailConfirmed" boolean NOT NULL,
                               "PasswordHash" text,
                               "SecurityStamp" text,
                               "ConcurrencyStamp" text,
                               "PhoneNumber" text,
                               "PhoneNumberConfirmed" boolean NOT NULL,
                               "TwoFactorEnabled" boolean NOT NULL,
                               "LockoutEnd" timestamp with time zone,
                               "LockoutEnabled" boolean NOT NULL,
                               "AccessFailedCount" integer NOT NULL,
                               CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
);

CREATE TABLE "Counts" (
                          "Id" serial NOT NULL,
                          "Code" text NOT NULL,
                          "Description" text NOT NULL,
                          "Value" integer NOT NULL,
                          CONSTRAINT "PK_Counts" PRIMARY KEY ("Id")
);

CREATE TABLE "Directions" (
                              "Id" serial NOT NULL,
                              "Name" text NOT NULL,
                              "Code" text NOT NULL,
                              "Description" text NOT NULL,
                              "BasePrice" numeric(18,2) NOT NULL,
                              CONSTRAINT "PK_Directions" PRIMARY KEY ("Id")
);

CREATE TABLE "DiscountRules" (
                                 "Id" serial NOT NULL,
                                 "MinQuantity" integer NOT NULL,
                                 "MaxQuantity" integer,
                                 "DiscountRate" numeric(5,4) NOT NULL,
                                 CONSTRAINT "PK_DiscountRules" PRIMARY KEY ("Id")
);

CREATE TABLE "Grades" (
                          "Id" serial NOT NULL,
                          "Name" text NOT NULL,
                          "Code" text NOT NULL,
                          "Description" text NOT NULL,
                          CONSTRAINT "PK_Grades" PRIMARY KEY ("Id")
);

CREATE TABLE "Invites" (
                           "Id" uuid NOT NULL,
                           "Email" text NOT NULL,
                           "InviteCode" text NOT NULL,
                           "IsUsed" boolean NOT NULL,
                           "CreatedAt" timestamp with time zone NOT NULL,
                           CONSTRAINT "PK_Invites" PRIMARY KEY ("Id")
);

CREATE TABLE "MailingSettings" (
                                   "Id" serial NOT NULL,
                                   "MailingCode" text NOT NULL,
                                   "IsEnabled" boolean NOT NULL,
                                   "Subject" text NOT NULL,
                                   "Body" text NOT NULL,
                                   CONSTRAINT "PK_MailingSettings" PRIMARY KEY ("Id")
);

CREATE TABLE "Orders" (
                          "Id" serial NOT NULL,
                          "UserId" text NOT NULL,
                          "Role" text NOT NULL,
                          "Quantity" integer NOT NULL,
                          "UnitPrice" numeric(18,2) NOT NULL,
                          "TotalPrice" numeric(18,2) NOT NULL,
                          "DiscountRate" numeric(18,2) NOT NULL,
                          "DiscountedTotal" numeric(18,2) NOT NULL,
                          "Status" integer NOT NULL,
                          "PaidAt" timestamp with time zone,
                          "PaymentId" character varying(64),
                          CONSTRAINT "PK_Orders" PRIMARY KEY ("Id")
);

CREATE TABLE "Persents" (
                            "Id" serial NOT NULL,
                            "Code" text NOT NULL,
                            "Description" text NOT NULL,
                            "Value" double precision NOT NULL,
                            CONSTRAINT "PK_Persents" PRIMARY KEY ("Id")
);

CREATE TABLE "Times" (
                         "Id" serial NOT NULL,
                         "Code" text NOT NULL,
                         "Description" text NOT NULL,
                         "Value" double precision NOT NULL,
                         CONSTRAINT "PK_Times" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetRoleClaims" (
                                    "Id" serial NOT NULL,
                                    "RoleId" text NOT NULL,
                                    "ClaimType" text,
                                    "ClaimValue" text,
                                    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
                                    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AdministratorProfiles" (
                                         "Id" serial NOT NULL,
                                         "Email" character varying(200) NOT NULL,
                                         "UserId" text NOT NULL,
                                         CONSTRAINT "PK_AdministratorProfiles" PRIMARY KEY ("Id"),
                                         CONSTRAINT "FK_AdministratorProfiles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
                                    "Id" serial NOT NULL,
                                    "UserId" text NOT NULL,
                                    "ClaimType" text,
                                    "ClaimValue" text,
                                    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
                                    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
                                    "LoginProvider" text NOT NULL,
                                    "ProviderKey" text NOT NULL,
                                    "ProviderDisplayName" text,
                                    "UserId" text NOT NULL,
                                    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
                                    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
                                   "UserId" text NOT NULL,
                                   "RoleId" text NOT NULL,
                                   CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
                                   CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
                                   CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
                                    "UserId" text NOT NULL,
                                    "LoginProvider" text NOT NULL,
                                    "Name" text NOT NULL,
                                    "Value" text,
                                    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
                                    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "CompanyProfiles" (
                                   "Id" serial NOT NULL,
                                   "FullName" character varying(200) NOT NULL,
                                   "LegalAddress" character varying(256),
                                   "INN" character varying(12) NOT NULL,
                                   "Kpp" character varying(9),
                                   "Email" character varying(200) NOT NULL,
                                   "UserId" text NOT NULL,
                                   CONSTRAINT "PK_CompanyProfiles" PRIMARY KEY ("Id"),
                                   CONSTRAINT "FK_CompanyProfiles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PersonProfiles" (
                                  "Id" serial NOT NULL,
                                  "FullName" character varying(200) NOT NULL,
                                  "Email" character varying(200) NOT NULL,
                                  "UserId" text NOT NULL,
                                  CONSTRAINT "PK_PersonProfiles" PRIMARY KEY ("Id"),
                                  CONSTRAINT "FK_PersonProfiles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RefreshTokens" (
                                 "Id" serial NOT NULL,
                                 "Token" text NOT NULL,
                                 "Expires" timestamp with time zone NOT NULL,
                                 "Created" timestamp with time zone NOT NULL,
                                 "Revoked" timestamp with time zone,
                                 "UserId" text NOT NULL,
                                 CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
                                 CONSTRAINT "FK_RefreshTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Topics" (
                          "Id" serial NOT NULL,
                          "Name" text NOT NULL,
                          "Description" text NOT NULL,
                          "DirectionId" integer NOT NULL,
                          CONSTRAINT "PK_Topics" PRIMARY KEY ("Id"),
                          CONSTRAINT "FK_Topics_Directions_DirectionId" FOREIGN KEY ("DirectionId") REFERENCES "Directions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AnswerTimes" (
                               "Id" serial NOT NULL,
                               "GradeId" integer NOT NULL,
                               "Average" double precision NOT NULL,
                               "Min" double precision NOT NULL,
                               "Max" double precision NOT NULL,
                               CONSTRAINT "PK_AnswerTimes" PRIMARY KEY ("Id"),
                               CONSTRAINT "FK_AnswerTimes_Grades_GradeId" FOREIGN KEY ("GradeId") REFERENCES "Grades" ("Id") ON DELETE CASCADE
);

CREATE TABLE "GradeLevels" (
                               "Id" serial NOT NULL,
                               "GradeId" integer NOT NULL,
                               "Min" double precision NOT NULL,
                               "Max" double precision NOT NULL,
                               CONSTRAINT "PK_GradeLevels" PRIMARY KEY ("Id"),
                               CONSTRAINT "FK_GradeLevels_Grades_GradeId" FOREIGN KEY ("GradeId") REFERENCES "Grades" ("Id") ON DELETE CASCADE
);

CREATE TABLE "GradeRelations" (
                                  "Id" serial NOT NULL,
                                  "StartId" integer,
                                  "EndId" integer,
                                  CONSTRAINT "PK_GradeRelations" PRIMARY KEY ("Id"),
                                  CONSTRAINT "FK_GradeRelations_Grades_EndId" FOREIGN KEY ("EndId") REFERENCES "Grades" ("Id"),
                                  CONSTRAINT "FK_GradeRelations_Grades_StartId" FOREIGN KEY ("StartId") REFERENCES "Grades" ("Id")
);

CREATE TABLE "Weights" (
                           "Id" serial NOT NULL,
                           "GradeId" integer NOT NULL,
                           "Min" double precision NOT NULL,
                           "Max" double precision NOT NULL,
                           CONSTRAINT "PK_Weights" PRIMARY KEY ("Id"),
                           CONSTRAINT "FK_Weights_Grades_GradeId" FOREIGN KEY ("GradeId") REFERENCES "Grades" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Tokens" (
                          "Id" uuid NOT NULL,
                          "DirectionId" integer NOT NULL,
                          "Status" integer NOT NULL,
                          "UnitPrice" numeric(18,2) NOT NULL,
                          "PurchaseDate" timestamp with time zone NOT NULL,
                          "OrderId" integer,
                          "EmployeeUserId" text,
                          "EmployeeId" text,
                          "PersonUserId" text,
                          "PersonId" text,
                          "Score" double precision,
                          "CertificateUrl" text,
                          CONSTRAINT "PK_Tokens" PRIMARY KEY ("Id"),
                          CONSTRAINT "FK_Tokens_AspNetUsers_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "AspNetUsers" ("Id"),
                          CONSTRAINT "FK_Tokens_AspNetUsers_PersonId" FOREIGN KEY ("PersonId") REFERENCES "AspNetUsers" ("Id"),
                          CONSTRAINT "FK_Tokens_Directions_DirectionId" FOREIGN KEY ("DirectionId") REFERENCES "Directions" ("Id") ON DELETE CASCADE,
                          CONSTRAINT "FK_Tokens_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id")
);

CREATE TABLE "EmployeeProfiles" (
                                    "Id" serial NOT NULL,
                                    "FullName" character varying(200) NOT NULL,
                                    "Email" character varying(200) NOT NULL,
                                    "UserId" text NOT NULL,
                                    "CompanyProfileId" integer,
                                    CONSTRAINT "PK_EmployeeProfiles" PRIMARY KEY ("Id"),
                                    CONSTRAINT "FK_EmployeeProfiles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
                                    CONSTRAINT "FK_EmployeeProfiles_CompanyProfiles_CompanyProfileId" FOREIGN KEY ("CompanyProfileId") REFERENCES "CompanyProfiles" ("Id") ON DELETE SET NULL
);

CREATE TABLE "Questions" (
                             "Id" serial NOT NULL,
                             "Content" text NOT NULL,
                             "TopicId" integer NOT NULL,
                             "Weight" double precision NOT NULL,
                             "IsMultipleChoice" boolean NOT NULL,
                             CONSTRAINT "PK_Questions" PRIMARY KEY ("Id"),
                             CONSTRAINT "FK_Questions_Topics_TopicId" FOREIGN KEY ("TopicId") REFERENCES "Topics" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Reports" (
                           "Id" serial NOT NULL,
                           "TokenId" uuid NOT NULL,
                           "Score" double precision NOT NULL,
                           "GradeId" integer NOT NULL,
                           "Image" bytea,
                           CONSTRAINT "PK_Reports" PRIMARY KEY ("Id"),
                           CONSTRAINT "FK_Reports_Grades_GradeId" FOREIGN KEY ("GradeId") REFERENCES "Grades" ("Id") ON DELETE CASCADE,
                           CONSTRAINT "FK_Reports_Tokens_TokenId" FOREIGN KEY ("TokenId") REFERENCES "Tokens" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Sessions" (
                            "Id" serial NOT NULL,
                            "TokenId" uuid NOT NULL,
                            "StartTime" timestamp with time zone NOT NULL,
                            "EndTime" timestamp with time zone,
                            "Score" double precision NOT NULL,
                            "EmployeeUserId" text,
                            "EmployeeId" text,
                            "PersonUserId" text,
                            "PersonId" text,
                            CONSTRAINT "PK_Sessions" PRIMARY KEY ("Id"),
                            CONSTRAINT "FK_Sessions_AspNetUsers_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "AspNetUsers" ("Id"),
                            CONSTRAINT "FK_Sessions_AspNetUsers_PersonId" FOREIGN KEY ("PersonId") REFERENCES "AspNetUsers" ("Id"),
                            CONSTRAINT "FK_Sessions_Tokens_TokenId" FOREIGN KEY ("TokenId") REFERENCES "Tokens" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Answers" (
                           "Id" serial NOT NULL,
                           "QuestionId" integer NOT NULL,
                           "Content" text NOT NULL,
                           "IsCorrect" boolean NOT NULL,
                           CONSTRAINT "PK_Answers" PRIMARY KEY ("Id"),
                           CONSTRAINT "FK_Answers_Questions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "Questions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserAnswers" (
                               "Id" serial NOT NULL,
                               "SessionId" integer NOT NULL,
                               "QuestionId" integer NOT NULL,
                               "TimeSpent" double precision NOT NULL,
                               "Score" double precision NOT NULL,
                               "AnswerTime" timestamp with time zone NOT NULL,
                               CONSTRAINT "PK_UserAnswers" PRIMARY KEY ("Id"),
                               CONSTRAINT "FK_UserAnswers_Questions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "Questions" ("Id") ON DELETE CASCADE,
                               CONSTRAINT "FK_UserAnswers_Sessions_SessionId" FOREIGN KEY ("SessionId") REFERENCES "Sessions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserTopics" (
                              "Id" serial NOT NULL,
                              "SessionId" integer NOT NULL,
                              "TopicId" integer NOT NULL,
                              "GradeId" integer NOT NULL,
                              "Weight" double precision NOT NULL,
                              "IsFinished" boolean NOT NULL,
                              "WasPrevious" boolean NOT NULL,
                              "Actual" boolean NOT NULL,
                              "Count" integer NOT NULL,
                              CONSTRAINT "PK_UserTopics" PRIMARY KEY ("Id"),
                              CONSTRAINT "FK_UserTopics_Grades_GradeId" FOREIGN KEY ("GradeId") REFERENCES "Grades" ("Id") ON DELETE CASCADE,
                              CONSTRAINT "FK_UserTopics_Sessions_SessionId" FOREIGN KEY ("SessionId") REFERENCES "Sessions" ("Id") ON DELETE CASCADE,
                              CONSTRAINT "FK_UserTopics_Topics_TopicId" FOREIGN KEY ("TopicId") REFERENCES "Topics" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AdministratorProfiles_UserId" ON "AdministratorProfiles" ("UserId");

CREATE INDEX "IX_Answers_QuestionId" ON "Answers" ("QuestionId");

CREATE INDEX "IX_AnswerTimes_GradeId" ON "AnswerTimes" ("GradeId");

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

CREATE UNIQUE INDEX "IX_CompanyProfiles_UserId" ON "CompanyProfiles" ("UserId");

CREATE UNIQUE INDEX "IX_Directions_Name" ON "Directions" ("Name");

CREATE INDEX "IX_DiscountRules_MinQuantity" ON "DiscountRules" ("MinQuantity");

CREATE INDEX "IX_EmployeeProfiles_CompanyProfileId" ON "EmployeeProfiles" ("CompanyProfileId");

CREATE UNIQUE INDEX "IX_EmployeeProfiles_UserId" ON "EmployeeProfiles" ("UserId");

CREATE INDEX "IX_GradeLevels_GradeId" ON "GradeLevels" ("GradeId");

CREATE INDEX "IX_GradeRelations_EndId" ON "GradeRelations" ("EndId");

CREATE INDEX "IX_GradeRelations_StartId" ON "GradeRelations" ("StartId");

CREATE UNIQUE INDEX "IX_PersonProfiles_UserId" ON "PersonProfiles" ("UserId");

CREATE INDEX "IX_Questions_TopicId" ON "Questions" ("TopicId");

CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");

CREATE INDEX "IX_Reports_GradeId" ON "Reports" ("GradeId");

CREATE INDEX "IX_Reports_TokenId" ON "Reports" ("TokenId");

CREATE INDEX "IX_Sessions_EmployeeId" ON "Sessions" ("EmployeeId");

CREATE INDEX "IX_Sessions_PersonId" ON "Sessions" ("PersonId");

CREATE INDEX "IX_Sessions_TokenId" ON "Sessions" ("TokenId");

CREATE INDEX "IX_Tokens_DirectionId" ON "Tokens" ("DirectionId");

CREATE INDEX "IX_Tokens_EmployeeId" ON "Tokens" ("EmployeeId");

CREATE INDEX "IX_Tokens_OrderId" ON "Tokens" ("OrderId");

CREATE INDEX "IX_Tokens_PersonId" ON "Tokens" ("PersonId");

CREATE INDEX "IX_Topics_DirectionId" ON "Topics" ("DirectionId");

CREATE INDEX "IX_UserAnswers_QuestionId" ON "UserAnswers" ("QuestionId");

CREATE INDEX "IX_UserAnswers_SessionId" ON "UserAnswers" ("SessionId");

CREATE INDEX "IX_UserTopics_GradeId" ON "UserTopics" ("GradeId");

CREATE INDEX "IX_UserTopics_SessionId" ON "UserTopics" ("SessionId");

CREATE INDEX "IX_UserTopics_TopicId" ON "UserTopics" ("TopicId");

CREATE INDEX "IX_Weights_GradeId" ON "Weights" ("GradeId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250608161629_SyncWithModel', '9.0.4');

COMMIT;


/*  
Конфигурация Counts 
*/

INSERT INTO "Counts" ("Id", "Code", "Description", "Value") VALUES (2, 'Question', 'Количество вопросов в топике', 10);

/*  
Конфигурация Grades 
*/

INSERT INTO "Grades" ("Id", "Name", "Code", "Description") VALUES (1, 'Junior', 'Junior', 'Grade of Junior');
INSERT INTO "Grades" ("Id", "Name", "Code", "Description") VALUES (2, 'Middle', 'Middle', 'Grade of Middle');
INSERT INTO "Grades" ("Id", "Name", "Code", "Description") VALUES (3, 'Senior', 'Senior', 'Grade of Senior');

/*  
Конфигурация Persents 
*/

INSERT INTO "Persents" ("Id", "Code", "Description", "Value") VALUES (1, 'DecreaseLevel', 'Процент вопросов для закрытия уровня (/100)', 0.3);
INSERT INTO "Persents" ("Id", "Code", "Description", "Value") VALUES (2, 'RaiseData', 'Процент разницы веса для повышения уровня', 0.15);
INSERT INTO "Persents" ("Id", "Code", "Description", "Value") VALUES (3, 'IncreaseLevel', 'Процент вопросов для повышения уровня (/100)', 0.35);
INSERT INTO "Persents" ("Id", "Code", "Description", "Value") VALUES (4, 'Reasonable', 'Искуственная граница', 0.45);

/*  
Конфигурация Direction 
*/

INSERT INTO "Directions" ("Id", "Name", "Code", "Description", "BasePrice") VALUES (1, 'QA', 'QA', 'Quality assurance', 1000);
INSERT INTO "Directions" ("Id", "Name", "Code", "Description", "BasePrice") VALUES (2, 'SA', 'SA', 'System analises', 2000);
INSERT INTO "Directions" ("Id", "Name", "Code", "Description", "BasePrice") VALUES (3, 'DevOps', 'DevOps', 'Development and Operations', 2000);
INSERT INTO "Directions" ("Id", "Name", "Code", "Description", "BasePrice") VALUES (4, 'ML', 'ML', 'ML Engineer', 2000);


/*  
Конфигурация GradeLevels 
*/

INSERT INTO "GradeLevels" ("Id", "GradeId", "Min", "Max") VALUES (1, 1, 0, 0.4);
INSERT INTO "GradeLevels" ("Id", "GradeId", "Min", "Max") VALUES (2, 2, 0.4, 0.7);
INSERT INTO "GradeLevels" ("Id", "GradeId", "Min", "Max") VALUES (3, 3, 0.7, 1.01);

/*  
Конфигурация GradeRelations 
*/

INSERT INTO "GradeRelations" ("Id", "StartId", "EndId") VALUES (2, null, 1);
INSERT INTO "GradeRelations" ("Id", "StartId", "EndId") VALUES (3, 1, 2);
INSERT INTO "GradeRelations" ("Id", "StartId", "EndId") VALUES (4, 2, 3);
INSERT INTO "GradeRelations" ("Id", "StartId", "EndId") VALUES (5, 3, null);

/*  
Конфигурация AnswerTimes 
*/

INSERT INTO "AnswerTimes" ("Id", "GradeId", "Average", "Min", "Max") VALUES (1, 1, 60, 0.95, 1.05);
INSERT INTO "AnswerTimes" ("Id", "GradeId", "Average", "Min", "Max") VALUES (2, 2, 90, 0.93, 1.07);
INSERT INTO "AnswerTimes" ("Id", "GradeId", "Average", "Min", "Max") VALUES (3, 3, 120, 0.9, 1.1);


/*  
Конфигурация Weights 
*/

INSERT INTO "Weights" ("Id", "GradeId", "Min", "Max") VALUES (1, 1, 0.01, 0.4);
INSERT INTO "Weights" ("Id", "GradeId", "Min", "Max") VALUES (2, 2, 0.4, 0.6);
INSERT INTO "Weights" ("Id", "GradeId", "Min", "Max") VALUES (3, 3, 0.6, 1.0);

/*  
Конфигурация DiscountRules 
*/

INSERT INTO "DiscountRules" ("Id", "MinQuantity", "MaxQuantity", "DiscountRate") VALUES (1, 1, 49, 0);
INSERT INTO "DiscountRules" ("Id", "MinQuantity", "MaxQuantity", "DiscountRate") VALUES (2, 50, 99, 0.10);
INSERT INTO "DiscountRules" ("Id", "MinQuantity", "MaxQuantity", "DiscountRate") VALUES (3, 100, null, 0.15);

/*  
Конфигурация MailingSettings 
*/

INSERT INTO "MailingSettings" ("Id", "MailingCode", "IsEnabled", "Subject", "Body") VALUES (1, 'ResetPassword', true, 'Сброс пароля', 'Здравствуйте! Чтобы задать новый пароль, перейдите по ссылке: {link}');

/*  
Конфигурация Times 
*/

INSERT INTO "Times" ("Id", "Code", "Description", "Value") VALUES (1, 'SessionDuration', 'Длительность сессии (мин.)', 90);


/*  
Конфигурация Topics 
*/

INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Основы тестирования', 'Основы тестирования', 1);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Инструменты тестирования', 'Инструменты тестирования', 1);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Методологии разработки и тестирования', 'Методологии разработки и тестирования', 1);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Документация и процессы', 'Документация и процессы', 1);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Типы тестирования', 'Типы тестирования', 1);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Языки программирования', 'Языки программирования', 1);

INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Сбор и управление требованиями', 'Сбор и управление требованиями', 2);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Моделирование процессов и систем', 'Моделирование процессов и систем', 2);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Технические аспекты и интеграции', 'Технические аспекты и интеграции', 2);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Методологии разработки и участие в процессах', 'Методологии разработки и участие в процессах', 2);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Работа со стейкхолдерами', 'Работа со стейкхолдерами', 2);


INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('CI/CD и Delivery-стратегии','CI/CD и Delivery-стратегии',3);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Контейнеризация (Docker, Compose)','Контейнеризация (Docker, Compose)',3);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Kubernetes и оркестрация','Kubernetes и оркестрация',3);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('IaC (Terraform, Ansible, Pulumi)','IaC (Terraform, Ansible, Pulumi)',3);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Linux, Shell и процессы','Linux, Shell и процессы',3);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Мониторинг, алерты и Observability','Мониторинг, алерты и Observability',3);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Логирование и трейсинг (EFK, Loki, OTel)','Логирование и трейсинг (EFK, Loki, OTel)',3);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Security & IAM (Vault, TLS, SSH)','Security & IAM (Vault, TLS, SSH)',3);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Cloud-инфраструктура (AWS, GCP, Azure)','Cloud-инфраструктура (AWS, GCP, Azure)',3);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Сетевые основы и взаимодействие','Сетевые основы и взаимодействие',3);


INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Базовые алгоритмы и задачи машинного обучения','Базовые алгоритмы и задачи машинного обучения',4);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Метрики и оценка моделей','Метрики и оценка моделей',4);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Работа с данными и препроцессинг','Работа с данными и препроцессинг',4);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Оверфиттинг, андерфиттинг и качество модели','Оверфиттинг, андерфиттинг и качество модели',4);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Базовые принципы продакшена моделей (MLOps intro)','Базовые принципы продакшена моделей (MLOps intro)',4);
INSERT INTO "Topics" ("Name", "Description", "DirectionId") VALUES ('Основы explainability и интерпретации','Основы explainability и интерпретации',4);