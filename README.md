# 📅 Планувальник записів (Appointment Scheduler API)

REST API сервіс для керування розкладом фахівців та бронювання часу клієнтами. Проєкт реалізує **Завдання 31** з використанням сучасних інженерних практик .NET.

## 🚀 Основні можливості (Вимоги)
- **Отримання доступних слотів:** Розрахунок вільного часу фахівця на певну дату з вирахуванням уже зайнятих слотів.
- **Бронювання запису:** Створення зустрічі з автоматичною перевіркою на перетини (Overlap Detection) та дотриманням робочих годин фахівця (Business Hours).
- **Скасування запису:** Бізнес-правило, яке дозволяє скасовувати або переносити зустріч не пізніше, ніж за 2 години до її початку.

## 🏗 Архітектура та Технології

Проєкт побудований за принципами **Clean Architecture** (Чистої архітектури) та **Domain-Driven Design (DDD)**.



- **Платформа:** .NET 10 / C# 14
- **Паттерни:** CQRS (через MediatR), Repository Pattern
- **База даних:** PostgreSQL + Entity Framework Core
- **Документація API:** Swagger / OpenAPI

## 🧪 Забезпечення якості (QA) та Тестування
Проєкт має багаторівневу стратегію тестування:

1. **Модульні тести (Unit Tests):** Швидкі тести (xUnit + NSubstitute + Shouldly) для перевірки бізнес-логіки та Use Cases (ізольовано від бази даних).
2. **Інтеграційні тести:** Використовують `WebApplicationFactory` та **Testcontainers** для автоматичного підняття ізольованого контейнера PostgreSQL. БД наповнюється 10 000 записами для перевірки реальної поведінки системи.
3. **Навантажувальне тестування (k6):** Скрипти на JavaScript для стрес-тестування одночасного бронювання (Race Conditions) та перевірки продуктивності (Load Testing).
4. **CI/CD:** Налаштований GitHub Actions Workflow для автоматичної збірки, тестування та перевірки Quality Gates (покриття коду > 80%).

## ⚙️ Вимоги для запуску
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Обов'язково запущений для роботи бази даних та інтеграційних тестів)
- [k6](https://k6.io/docs/get-started/installation/) (Опціонально, для запуску тестів продуктивності)

## 🛠 Як запустити локально

1. Клонуйте репозиторій.
2. Переконайтеся, що Docker запущений.
3. Запустіть API з кореневої папки:
   ```bash
   dotnet run --project src/Api/Api.csproj
4. Відкрийте Swagger у браузері: http://localhost:<port>/swagger

##  Як запустити тести та згенерувати звіт покриття

Щоб запустити всі тести та зібрати метрики покриття коду (Code Coverage):

# 1. Запуск тестів
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./TestResults"

# 2. Генерація HTML-звіту (потрібен встановлений dotnet-reportgenerator-globaltool)
reportgenerator -reports:"./TestResults/**/*.xml" -targetdir:"./CoverageReport" -reporttypes:Html

Після виконання відкрийте файл CoverageReport/index.html у браузері, щоб переглянути детальну статистику покриття бізнес-логіки.

## 🏎 Як запустити тести продуктивності (k6)

Bash
k6 run tests/PerformanceTests/test-booking.js