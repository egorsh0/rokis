create table if not exists "__EFMigrationsHistory"
(
    "MigrationId"    varchar(150) not null
        constraint "PK___EFMigrationsHistory"
            primary key,
    "ProductVersion" varchar(32)  not null
);

create table if not exists "AspNetRoles"
(
    "Id"               text not null
        constraint "PK_AspNetRoles"
            primary key,
    "Name"             varchar(256),
    "NormalizedName"   varchar(256),
    "ConcurrencyStamp" text
);

create unique index if not exists "RoleNameIndex"
    on "AspNetRoles" ("NormalizedName");

create table if not exists "AspNetUsers"
(
    "Id"                   text                     not null
        constraint "PK_AspNetUsers"
            primary key,
    "DisplayName"          varchar(255)             not null,
    "PasswordLastChanged"  timestamp with time zone not null,
    "UserName"             varchar(256),
    "NormalizedUserName"   varchar(256),
    "Email"                varchar(256),
    "NormalizedEmail"      varchar(256),
    "EmailConfirmed"       boolean                  not null,
    "PasswordHash"         text,
    "SecurityStamp"        text,
    "ConcurrencyStamp"     text,
    "PhoneNumber"          text,
    "PhoneNumberConfirmed" boolean                  not null,
    "TwoFactorEnabled"     boolean                  not null,
    "LockoutEnd"           timestamp with time zone,
    "LockoutEnabled"       boolean                  not null,
    "AccessFailedCount"    integer                  not null
);

create index if not exists "EmailIndex"
    on "AspNetUsers" ("NormalizedEmail");

create unique index if not exists "UserNameIndex"
    on "AspNetUsers" ("NormalizedUserName");

create table if not exists "Counts"
(
    "Id"          serial
        constraint "PK_Counts"
            primary key,
    "Code"        text    not null,
    "Description" text    not null,
    "Value"       integer not null
);

create table if not exists "Directions"
(
    "Id"          serial
        constraint "PK_Directions"
            primary key,
    "Name"        text           not null,
    "Code"        text           not null,
    "Description" text           not null,
    "BasePrice"   numeric(18, 2) not null
);

create unique index if not exists "IX_Directions_Name"
    on "Directions" ("Name");

create table if not exists "DiscountRules"
(
    "Id"           serial
        constraint "PK_DiscountRules"
            primary key,
    "MinQuantity"  integer       not null,
    "MaxQuantity"  integer,
    "DiscountRate" numeric(5, 4) not null
);

create index if not exists "IX_DiscountRules_MinQuantity"
    on "DiscountRules" ("MinQuantity");

create table if not exists "Grades"
(
    "Id"          serial
        constraint "PK_Grades"
            primary key,
    "Name"        text not null,
    "Code"        text not null,
    "Description" text not null
);

create table if not exists "Invites"
(
    "Id"         uuid                     not null
        constraint "PK_Invites"
            primary key,
    "Email"      text                     not null,
    "InviteCode" text                     not null,
    "IsUsed"     boolean                  not null,
    "CreatedAt"  timestamp with time zone not null
);

create table if not exists "MailingSettings"
(
    "Id"          serial
        constraint "PK_MailingSettings"
            primary key,
    "MailingCode" text    not null,
    "IsEnabled"   boolean not null,
    "Subject"     text    not null,
    "Body"        text    not null
);

create table if not exists "Orders"
(
    "Id"              serial
        constraint "PK_Orders"
            primary key,
    "UserId"          text           not null,
    "Role"            text           not null,
    "Quantity"        integer        not null,
    "UnitPrice"       numeric(18, 2) not null,
    "TotalPrice"      numeric(18, 2) not null,
    "DiscountRate"    numeric(18, 2) not null,
    "DiscountedTotal" numeric(18, 2) not null,
    "Status"          integer        not null,
    "PaidAt"          timestamp with time zone,
    "PaymentId"       varchar(64)
);

create table if not exists "Persents"
(
    "Id"          serial
        constraint "PK_Persents"
            primary key,
    "Code"        text             not null,
    "Description" text             not null,
    "Value"       double precision not null
);

create table if not exists "AspNetRoleClaims"
(
    "Id"         serial
        constraint "PK_AspNetRoleClaims"
            primary key,
    "RoleId"     text not null
        constraint "FK_AspNetRoleClaims_AspNetRoles_RoleId"
            references "AspNetRoles"
            on delete cascade,
    "ClaimType"  text,
    "ClaimValue" text
);

create index if not exists "IX_AspNetRoleClaims_RoleId"
    on "AspNetRoleClaims" ("RoleId");

create table if not exists "AdministratorProfiles"
(
    "Id"     serial
        constraint "PK_AdministratorProfiles"
            primary key,
    "Email"  varchar(200) not null,
    "UserId" text         not null
        constraint "FK_AdministratorProfiles_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade
);

create index if not exists "IX_AdministratorProfiles_UserId"
    on "AdministratorProfiles" ("UserId");

create table if not exists "AspNetUserClaims"
(
    "Id"         serial
        constraint "PK_AspNetUserClaims"
            primary key,
    "UserId"     text not null
        constraint "FK_AspNetUserClaims_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade,
    "ClaimType"  text,
    "ClaimValue" text
);

create index if not exists "IX_AspNetUserClaims_UserId"
    on "AspNetUserClaims" ("UserId");

create table if not exists "AspNetUserLogins"
(
    "LoginProvider"       text not null,
    "ProviderKey"         text not null,
    "ProviderDisplayName" text,
    "UserId"              text not null
        constraint "FK_AspNetUserLogins_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade,
    constraint "PK_AspNetUserLogins"
        primary key ("LoginProvider", "ProviderKey")
);

create index if not exists "IX_AspNetUserLogins_UserId"
    on "AspNetUserLogins" ("UserId");

create table if not exists "AspNetUserRoles"
(
    "UserId" text not null
        constraint "FK_AspNetUserRoles_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade,
    "RoleId" text not null
        constraint "FK_AspNetUserRoles_AspNetRoles_RoleId"
            references "AspNetRoles"
            on delete cascade,
    constraint "PK_AspNetUserRoles"
        primary key ("UserId", "RoleId")
);

create index if not exists "IX_AspNetUserRoles_RoleId"
    on "AspNetUserRoles" ("RoleId");

create table if not exists "AspNetUserTokens"
(
    "UserId"        text not null
        constraint "FK_AspNetUserTokens_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade,
    "LoginProvider" text not null,
    "Name"          text not null,
    "Value"         text,
    constraint "PK_AspNetUserTokens"
        primary key ("UserId", "LoginProvider", "Name")
);

create table if not exists "CompanyProfiles"
(
    "Id"           serial
        constraint "PK_CompanyProfiles"
            primary key,
    "FullName"     varchar(200) not null,
    "LegalAddress" varchar(256),
    "INN"          varchar(12)  not null,
    "Kpp"          varchar(9),
    "Email"        varchar(200) not null,
    "UserId"       text         not null
        constraint "FK_CompanyProfiles_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade
);

create unique index if not exists "IX_CompanyProfiles_UserId"
    on "CompanyProfiles" ("UserId");

create table if not exists "PersonProfiles"
(
    "Id"       serial
        constraint "PK_PersonProfiles"
            primary key,
    "FullName" varchar(200) not null,
    "Email"    varchar(200) not null,
    "UserId"   text         not null
        constraint "FK_PersonProfiles_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade
);

create unique index if not exists "IX_PersonProfiles_UserId"
    on "PersonProfiles" ("UserId");

create table if not exists "Topics"
(
    "Id"          serial
        constraint "PK_Topics"
            primary key,
    "Name"        text    not null,
    "Description" text    not null,
    "DirectionId" integer not null
        constraint "FK_Topics_Directions_DirectionId"
            references "Directions"
            on delete cascade
);

create index if not exists "IX_Topics_DirectionId"
    on "Topics" ("DirectionId");

create table if not exists "AnswerTimes"
(
    "Id"      serial
        constraint "PK_AnswerTimes"
            primary key,
    "GradeId" integer          not null
        constraint "FK_AnswerTimes_Grades_GradeId"
            references "Grades"
            on delete cascade,
    "Average" double precision not null,
    "Min"     double precision not null,
    "Max"     double precision not null
);

create index if not exists "IX_AnswerTimes_GradeId"
    on "AnswerTimes" ("GradeId");

create table if not exists "GradeLevels"
(
    "Id"      serial
        constraint "PK_GradeLevels"
            primary key,
    "GradeId" integer                    not null
        constraint "FK_GradeLevels_Grades_GradeId"
            references "Grades"
            on delete cascade,
    "Level"   double precision           not null,
    "Min"     double precision default 0 not null,
    "Max"     double precision default 0 not null
);

create index if not exists "IX_GradeLevels_GradeId"
    on "GradeLevels" ("GradeId");

create table if not exists "GradeRelations"
(
    "Id"      serial
        constraint "PK_GradeRelations"
            primary key,
    "StartId" integer
        constraint "FK_GradeRelations_Grades_StartId"
            references "Grades",
    "EndId"   integer
        constraint "FK_GradeRelations_Grades_EndId"
            references "Grades"
);

create index if not exists "IX_GradeRelations_EndId"
    on "GradeRelations" ("EndId");

create index if not exists "IX_GradeRelations_StartId"
    on "GradeRelations" ("StartId");

create table if not exists "Weights"
(
    "Id"      serial
        constraint "PK_Weights"
            primary key,
    "GradeId" integer          not null
        constraint "FK_Weights_Grades_GradeId"
            references "Grades"
            on delete cascade,
    "Min"     double precision not null,
    "Max"     double precision not null
);

create index if not exists "IX_Weights_GradeId"
    on "Weights" ("GradeId");

create table if not exists "Tokens"
(
    "Id"             uuid                     not null
        constraint "PK_Tokens"
            primary key,
    "DirectionId"    integer                  not null
        constraint "FK_Tokens_Directions_DirectionId"
            references "Directions"
            on delete cascade,
    "Status"         integer                  not null,
    "UnitPrice"      numeric(18, 2)           not null,
    "PurchaseDate"   timestamp with time zone not null,
    "OrderId"        integer
        constraint "FK_Tokens_Orders_OrderId"
            references "Orders",
    "EmployeeUserId" text,
    "EmployeeId"     text
        constraint "FK_Tokens_AspNetUsers_EmployeeId"
            references "AspNetUsers",
    "PersonUserId"   text,
    "PersonId"       text
        constraint "FK_Tokens_AspNetUsers_PersonId"
            references "AspNetUsers",
    "Score"          double precision,
    "CertificateUrl" text
);

create index if not exists "IX_Tokens_DirectionId"
    on "Tokens" ("DirectionId");

create index if not exists "IX_Tokens_EmployeeId"
    on "Tokens" ("EmployeeId");

create index if not exists "IX_Tokens_OrderId"
    on "Tokens" ("OrderId");

create index if not exists "IX_Tokens_PersonId"
    on "Tokens" ("PersonId");

create table if not exists "EmployeeProfiles"
(
    "Id"               serial
        constraint "PK_EmployeeProfiles"
            primary key,
    "FullName"         varchar(200) not null,
    "Email"            varchar(200) not null,
    "UserId"           text         not null
        constraint "FK_EmployeeProfiles_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade,
    "CompanyProfileId" integer
        constraint "FK_EmployeeProfiles_CompanyProfiles_CompanyProfileId"
            references "CompanyProfiles"
            on delete set null
);

create index if not exists "IX_EmployeeProfiles_CompanyProfileId"
    on "EmployeeProfiles" ("CompanyProfileId");

create unique index if not exists "IX_EmployeeProfiles_UserId"
    on "EmployeeProfiles" ("UserId");

create table if not exists "Questions"
(
    "Id"               serial
        constraint "PK_Questions"
            primary key,
    "Content"          text             not null,
    "TopicId"          integer          not null
        constraint "FK_Questions_Topics_TopicId"
            references "Topics"
            on delete cascade,
    "Weight"           double precision not null,
    "IsMultipleChoice" boolean          not null
);

create index if not exists "IX_Questions_TopicId"
    on "Questions" ("TopicId");

create table if not exists "Reports"
(
    "Id"      serial
        constraint "PK_Reports"
            primary key,
    "TokenId" uuid             not null
        constraint "FK_Reports_Tokens_TokenId"
            references "Tokens"
            on delete cascade,
    "Score"   double precision not null,
    "GradeId" integer          not null
        constraint "FK_Reports_Grades_GradeId"
            references "Grades"
            on delete cascade,
    "Image"   bytea
);

create index if not exists "IX_Reports_GradeId"
    on "Reports" ("GradeId");

create index if not exists "IX_Reports_TokenId"
    on "Reports" ("TokenId");

create table if not exists "Sessions"
(
    "Id"             serial
        constraint "PK_Sessions"
            primary key,
    "TokenId"        uuid                     not null
        constraint "FK_Sessions_Tokens_TokenId"
            references "Tokens"
            on delete cascade,
    "StartTime"      timestamp with time zone not null,
    "EndTime"        timestamp with time zone,
    "Score"          double precision         not null,
    "EmployeeUserId" text,
    "EmployeeId"     text
        constraint "FK_Sessions_AspNetUsers_EmployeeId"
            references "AspNetUsers",
    "PersonUserId"   text,
    "PersonId"       text
        constraint "FK_Sessions_AspNetUsers_PersonId"
            references "AspNetUsers"
);

create index if not exists "IX_Sessions_EmployeeId"
    on "Sessions" ("EmployeeId");

create index if not exists "IX_Sessions_PersonId"
    on "Sessions" ("PersonId");

create index if not exists "IX_Sessions_TokenId"
    on "Sessions" ("TokenId");

create table if not exists "Answers"
(
    "Id"         serial
        constraint "PK_Answers"
            primary key,
    "QuestionId" integer not null
        constraint "FK_Answers_Questions_QuestionId"
            references "Questions"
            on delete cascade,
    "Content"    text    not null,
    "IsCorrect"  boolean not null
);

create index if not exists "IX_Answers_QuestionId"
    on "Answers" ("QuestionId");

create table if not exists "UserAnswers"
(
    "Id"         serial
        constraint "PK_UserAnswers"
            primary key,
    "SessionId"  integer                  not null
        constraint "FK_UserAnswers_Sessions_SessionId"
            references "Sessions"
            on delete cascade,
    "QuestionId" integer                  not null
        constraint "FK_UserAnswers_Questions_QuestionId"
            references "Questions"
            on delete cascade,
    "TimeSpent"  double precision         not null,
    "Score"      double precision         not null,
    "AnswerTime" timestamp with time zone not null
);

create index if not exists "IX_UserAnswers_QuestionId"
    on "UserAnswers" ("QuestionId");

create index if not exists "IX_UserAnswers_SessionId"
    on "UserAnswers" ("SessionId");

create table if not exists "UserTopics"
(
    "Id"          serial
        constraint "PK_UserTopics"
            primary key,
    "SessionId"   integer          not null
        constraint "FK_UserTopics_Sessions_SessionId"
            references "Sessions"
            on delete cascade,
    "TopicId"     integer          not null
        constraint "FK_UserTopics_Topics_TopicId"
            references "Topics"
            on delete cascade,
    "GradeId"     integer          not null
        constraint "FK_UserTopics_Grades_GradeId"
            references "Grades"
            on delete cascade,
    "Weight"      double precision not null,
    "IsFinished"  boolean          not null,
    "WasPrevious" boolean          not null,
    "Actual"      boolean          not null,
    "Count"       integer          not null
);

create index if not exists "IX_UserTopics_GradeId"
    on "UserTopics" ("GradeId");

create index if not exists "IX_UserTopics_SessionId"
    on "UserTopics" ("SessionId");

create index if not exists "IX_UserTopics_TopicId"
    on "UserTopics" ("TopicId");

create table if not exists "Times"
(
    "Id"          serial
        constraint "PK_Times"
            primary key,
    "Code"        text             not null,
    "Description" text             not null,
    "Value"       double precision not null
);

create table if not exists "RefreshTokens"
(
    "Id"      serial
        constraint "PK_RefreshTokens"
            primary key,
    "Token"   text                     not null,
    "Expires" timestamp with time zone not null,
    "Created" timestamp with time zone not null,
    "Revoked" timestamp with time zone,
    "UserId"  text                     not null
        constraint "FK_RefreshTokens_AspNetUsers_UserId"
            references "AspNetUsers"
            on delete cascade
);

create index if not exists "IX_RefreshTokens_UserId"
    on "RefreshTokens" ("UserId");

/*  
Конфигурация Counts 
*/

INSERT INTO "Counts" ("Id", "Code", "Description", "Value") VALUES (1, 'Question', 'Количество вопросов в топике', 10);
INSERT INTO "Counts" ("Id", "Code", "Description", "Value") VALUES (2, 'RollingWindow', 'Скользящее окно', 5);

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
INSERT INTO "Persents" ("Id", "Code", "Description", "Value") VALUES (4, 'MandatoryQuestions', 'Процент обязательных вопросов по теме (/100)', 0.5);

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