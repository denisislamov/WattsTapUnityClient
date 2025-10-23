# WattsTap - Техническое задание для Notion

## 📋 Общая информация

**Платформа**: Telegram WebGL Mini App (включая мобильные Webview)  
**Версия Unity**: Unity 6  
**Тип проекта**: 2D tap-to-earn игра с элементами RPG

---

## 🎮 Основной игровой цикл (Core Loop)

1. **Тапы → Монеты + Опыт** - основная механика заработка
2. **Монеты → Улучшения** - апгрейд параметров (профит за тап, оффлайн бонус, количество ударов)
3. **Экипировка → Мерж → Прокачка** - улучшение снаряжения через систему редкости
4. **Опыт → Уровни** - открытие новых персонажей и получение наград
5. **Турниры** - соревнование за награды (монеты, бустеры, предметы)
6. **Рефералы** - награды от заработка друзей

---

## 🏗️ Архитектура проекта

### 📐 Общая схема архитектуры

```
┌─────────────────────────────────────────────────────────────┐
│                    TELEGRAM WEB APP                         │
│                  (Мобильный WebView + Desktop)              │
└─────────────────────────────────────────────────────────────┘
                              ↓ HTTPS
┌────────────────────────────────────────────────────────────┐
│                        FRONTEND LAYER                      │
│ ┌─────────────────────┐      ┌───────────────────────────┐ │
│ │   UNITY WebGL       │      │    React Web Layer        │ │
│ │   (Unity 6)         │      │   (Minimal Web UI)        │ │
│ │                     │      │                           │ │
│ │ • Gameplay          │      │ • Telegram Auth           │ │
│ │ • Game UI           │      │ • Wallet Integration      │ │
│ │ • Animations        │      │ • Deep Links              │ │
│ │ • VFX/Particles     │      │ • Share/Invite            │ │
│ │ • Sound/Music       │      │ • WebView Bridge          │ │
│ │ • Input Handler     │      │                           │ │
│ └─────────────────────┘      └───────────────────────────┘ │
│           ↕ Bridge (jslib/JavaScript Interop)              │
└────────────────────────────────────────────────────────────┘
                              ↓ HTTPS / WSS
┌─────────────���──────────────────────────────────────────────┐
│                       BACKEND LAYER                        │
│ ┌────────────────────────────────────────────────────────┐ │
│ │              REST API (Node.js + Express)              │ │
│ │                                                        │ │
│ │  • Authentication Service (Telegram Validation)        │ │
│ │  • Game Logic Service                                  │ │
│ │  • User Management                                     │ │
│ │  • Inventory & Items                                   │ │
│ │  • Tournament System                                   │ │
│ │  • Referral System                                     │ │
│ │  • Wallet Integration                                  │ │
│ │  • Analytics & Events                                  │ │
│ │                                                        │ │
│ │  CORS: Enabled для WebGL доменов                       │ │
│ │  Rate Limiting: Защита от злоупотреблений              │ │
│ └────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────┘
                              ↓ HTTPS
┌─────────────────────────────────────────────────────────────┐
│                       DATA LAYER                            │
│ ┌──────────────────┐  ┌──────────────────┐  ┌────────────┐  │
│ │   DB             │  │   ???            │  │  S3/CDN    │  │
│ │                  │  │                  │  │            │  │
│ │ • Users          │  │ • Sessions       │  │ • Assets   │  │
│ │ • Inventory      │  │ • Leaderboards   │  │ • Audio    │  │
│ │ • Transactions   │  │ • Game State     │  │ • Sprites  │  │
│ │ • Analytics      │  │ • Rate Limits    │  │ • Bundles  │  │
│ └──────────────────┘  └──────────────────┘  └────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              ↓ HTTPS
┌─────────────────────────────────────────────────────────────┐
│                    CDN + ADDRESSABLES                       │
│                                                             │
│  • Unity Addressables для динамической загрузки контента    │
│  • CloudFlare CDN / AWS CloudFront                          │
│  • Загрузка ассетов по мере необходимости                   │
│  • Кэширование для быстрой загрузки                         │
└─────────────────────────────────────────────────────────────┘
                              ↓ HTTPS
┌─────────────────────────────────────────────────────────────┐
│                    CI/CD PIPELINE                           │
│                                                             │
│  TeamCity / Jenkins / Unity Cloud Build / GitHub Actions    │
│  ├─ Unity Build (WebGL)                                     │
│  ├─ React Build                                             │
│  ├─ Backend Tests                                           │
│  ├─ Deploy to Staging (HTTPS)                               │
│  └─ Deploy to Production (HTTPS)                            │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔧 Технический стек

### Frontend

#### **Unity WebGL (Unity 6)**
**Назначение**: Основной геймплей и игровой интерфейс

**Ответственность**:
- Вся игровая логика и UI
- Анимации персонажей (Spine 2D / Unity Animator)
- VFX системы (частицы искр, конфетти, блики)
- Система звука и музыки
- Управление тапами и жестами
- Все игровые экраны (Mining, Inventory, Upgrades, Shop, Friends, Quests)
- Система мержа предметов
- Открытие сундуков
- Турнирные таблицы
- Профиль игрока

**Технологии**:
- Unity 6 (LTS)
- Unity WebGL Build Target
- Unity Addressables System
- TextMeshPro для UI текстов
- Unity UI Toolkit / uGUI
- Spine 2D Runtime для анимаций (опционально)
- DOTween для анимаций UI
- Unity Analytics

**Особенности для WebGL**:
- Memory Size: 256MB (оптимизировано для мобильных устройств)

#### **React Web Layer (Минимальная обёртка)**
**Назначение**: Интеграция с Telegram и web-специфичные функции

**Ответственность**:
- Telegram Web App SDK интеграция
- Аутентификация через Telegram
- Подключение криптокошельков (TON Keeper, TON Space)
- Deep Links и Share функционал
- Bridge между Unity и Telegram API
- Обёртка для Unity WebGL canvas

**Технологии**:
- React
- TypeScript
- Telegram Web App SDK
- TON Connect SDK
- ...

---

## 🔐 Безопасность и HTTPS

### Все коммуникации через HTTPS

**Frontend → Backend**: `https://api.wattstap.com`
- SSL/TLS сертификаты (Let's Encrypt / AWS Certificate Manager)
- HSTS заголовки
- Secure cookies

**CDN → Assets**: `https://cdn.wattstap.com`
- Подписанные URL для приватных ассетов

**WebGL Build**: `https://game.wattstap.com`
- Сертификат для домена
- Content Security Policy (CSP)

### Защита API

**Rate Limiting**:
- Тапы: максимум 10/секунду на пользователя
- API запросы: 100 запросов/минуту
- Логин: 5 попыток/5 минут

**Валидация на сервере**:
- Все игровые действия валидируются на бэкенде
- Проверка физических ограничений (stamina, cooldowns)
- Защита от модификации client-side данных

**Anti-Cheat**:
- Server authoritative логика
- Проверка таймингов действий
- Детектирование аномалий в поведении

---

## 📦 CDN и Addressables

### Unity Addressables System

**Назначение**: Динамическая загрузка ассетов для уменьшения начального размера билда

**Структура групп**:

1. **Initial Load** (встроено в билд):
   - Core UI элементы
   - Основной персонаж (Гном)
   - Базовые звуки
   - Шрифты
   - Размер: ~5-10 MB

2. **On Demand** (загружается по необходимости):
   - Дополнительные персонажи
   - Экипировка (иконки, 3D модели)
   - VFX эффекты
   - Музыкальные треки
   - Локализации

3. **Cached** (кэшируется локально):
   - Часто используемые предметы
   - UI иконки
   - Звуки действий

**CDN конфигурация**:
```
https://cdn.wattstap.com/addressables/
├── /WebGL/
│   ├── catalog.json
│   ├── initial_assets.bundle
│   ├── characters.bundle
│   ├── items.bundle
│   ├── vfx.bundle
│   └── audio.bundle
```

**Стратегия загрузки**:
- Compression: LZ4 (баланс размер/скорость)
- Build & Load Paths: Remote (CDN)
- Update Restriction: Can Change Post Release
- Cache Clear Behavior: Clear When New Content

**Преимущества**:
- Быстрая первоначальная загрузка (~10 MB вместо 50+ MB)
- Обновление контента без пересборки всего билда
- Загрузка ассетов по требованию (экономия трафика на мобильных)
- A/B тестирование контента

---

## 🚀 CI/CD Pipeline

### Варианты CI/CD систем

**Опции для выбора**:

1. **TeamCity**
   - Мощный enterprise решение
   - Отличная интеграция с Unity
   - Гибкая конфигурация build chains
   - Self-hosted на нашем сервере

2. **Jenkins**
   - Open-source решение
   - Большое сообщество и плагины
   - Полный контроль над процессом
   - Self-hosted на нашем сервере

3. **Unity Cloud Build**
   - Нативная интеграция с Unity
   - Простота настройки для Unity проектов
   - Автоматическое управление версиями Unity
   - Облачное решение + deploy на наш сервер

4. **GitHub Actions / GitLab CI**
   - Интеграция с Git репозиторием
   - Бесплатный tier для начала
   - YAML конфигурация
   - Deploy на наш сервер через SSH/FTP

### Общий Pipeline

**Этапы**:
1. **Build Stage**
   - Unity WebGL Build (Unity 6)
   - React Build (Production mode)
   - Backend Build (TypeScript → JavaScript)

2. **Test Stage**
   - Backend unit tests
   - Integration tests
   - Build validation

3. **Deploy Stage**
   - Unity WebGL → CDN (S3/CloudFlare)
   - React App → Web Server (Nginx)
   - Backend → Application Server (Node.js)
   - Database migrations

4. **Post-Deploy**
   - Health checks
   - Smoke tests
   - Rollback на предыдущую версию при ошибках

### Окружения

- **Development**: Continuous deployment при push в `develop`
- **Staging**: Manual approval для тестирования
- **Production**: Manual approval с review

### Deploy на наш сервер

**Методы деплоя**:
- SSH + rsync для файлов
- Docker containers (опционально)
- PM2 для управления Node.js процессами
- Nginx как reverse proxy
- Zero-downtime deployment (Blue-Green)

**HTTPS настройка**:
- Let's Encrypt для SSL сертификатов
- Автоматическое обновление сертификатов
- HTTP → HTTPS redirect
- HSTS заголовки

---

## 📊 Технические требования для контента

### 🎨 2D Спрайты и анимации

#### **Персонажи (2D)**

**Форматы**:
- **Spine 2D**: JSON + Atlas (рекомендуется для скелетной анимации)
- **Sprite Sheet**: PNG Sequence (для простых анимаций)

**Технические параметры**:
- **Atlas разрешение**: 2048x2048 max (для мобильных WebGL)
- **Формат текстур**: PNG-32 (с прозрачностью)
- **Компрессия**: ASTC / ETC2 для WebGL

**Анимации**:
- FPS: 24-30 fps (стандарт для 2D)
- Типы: Idle, Attack, Victory, Defeat
- Длительность: Idle 2-4 сек, Action 0.5-2 сек

**Spine 2D (если используется)**:
- Bones: 50-70 max на персонажа
- Mesh deformation для оптимизации (вместо frame-by-frame)
- IK (Inverse Kinematics) для сложных движений

**Sprite Sheets (если используется)**:
- Grid layout (равномерные фреймы)
- Оптимизация: только уникальные фреймы
- Padding: 2-4 пикселя между фреймами

**Оптимизация**:
- Sprite Atlas для группировки элементов
- Избегать излишней детализации
- Reuse текстур где возможно

#### **UI элементы (2D)**

**Форматы**: PNG / WebP
- Иконки: 256x256, 512x512 (для retina)
- Простые элементы: PNG-8
- Сложные элементы: PNG-24

**Адаптивность**:
- Основные соотношения: 19.5:9 и 16:9
- 9-slice scaling для масштабируемых элементов

**Состояния UI**:
- Кнопки: normal, pressed, disabled
- Progress bars: fillable sprites

#### **Предметы и иконки**

**Разрешение**: 256x256 (inventory), 512x512 (preview/detail)  
**Формат**: PNG-32

**VFX для UI**:
- Particles (конфетти, блики, искры)
- Glow effects для редкости Purple+

#### **Фоны**

**Формат**: JPG / PNG  
**Разрешение**: 1920x1080 → 2K max

---

### 🎲 3D Модели и ассеты

#### **Персонажи и объекты (3D)**

**Форматы**:
- **Исходные**: FBX / Blender / Maya
- **Импорт Unity**: FBX (рекомендуется)
- **Текстуры**: PNG / TGA

**Полигональные бюджеты**:
- Персонажи: 5,000 - 10,000 треугольников
- Оружие/Экипировка: 1,000 - 5,000 треугольников
- Props/Окружение: 500 - 5,000 треугольников

#### **LOD система**

**Уровни детализации**: 2-3 уровня (при необходимости)
- **LOD0**: 100% полигонов (вблизи камеры)
- **LOD1**: 50-60% полигонов (средняя дистанция)
- **LOD2**: 25-30% полигонов (дальняя дистанция / фон)

**Переключение**: Автоматическое на основе расстояния до камеры

#### **Текстуры (3D)**

**Разрешение**:
- Персонажи: 1024x1024 или 2048x2048 max
- Props/Items: 512x512 или 1024x1024
- Environment: 1024x1024 или 2048x2048

**PBR карты**:
- Albedo, Normal Map, Metallic/Roughness
- Ambient Occlusion (опционально)

**Оптимизация**:
- Texture atlases где возможно
- Компрессия: ASTC / ETC2 для WebGL
- Shared materials для одинаковых объектов

#### **Анимации (3D)**

**Формат**: Unity Animation Clips (FBX import)

**Параметры**:
- FPS: 30 fps (стандарт для мобильных)
- Типы: Idle, Action, Transition
- Длительность: Idle 2-4 сек, Action 0.5-2 сек

**Rigging**:
- Персонажи: 30-60 bones max
- Props: 5-15 bones max
- Rig Type: Humanoid / Generic

**Оптимизация**:
- Animation Compression: Optimal для WebGL
- Blend Trees для плавных переходов
- Animation Events для синхронизации

#### **Материалы и рендеринг**

**Шейдеры**: Standard Shader / URP Lit (избегать custom для WebGL)  
**Rendering**: Single-pass, batching-friendly, минимум transparency

#### **Экспорт чек-лист**

- ✅ Правильный scale (1 unit = 1 meter)
- ✅ Корректный pivot point
- ✅ Applied transforms
- ✅ Clean geometry (no n-gons, overlaps)
- ✅ Proper UV mapping
- ✅ Consistent naming

---

### 🎵 Звуки и музыка

#### **Музыка (BGM)**

**Формат**: MP3 / OGG Vorbis
- **Битрейт**: 128 kbps (баланс качество/размер)
- **Sample Rate**: 44.1 kHz
- **Channels**: Stereo
- **Loop**: Seamless loop points
- **Длительность**: 1-3 минуты (зацикленные)
- **Размер**: ~2-5 MB на трек

**Загрузка**: Через Addressables (не в initial build)

#### **Sound Effects (SFX)**

**Формат**: WAV → конвертация в WebAudio format
- **Sample Rate**: 22.05 kHz (достаточно для SFX)
- **Битрейт**: 16-bit
- **Channels**: Mono (экономия размера)
- **Длительность**: 0.1 - 2 секунды
- **Размер**: до 100 KB на звук

**Unity Audio Settings**:
- Compression Format: Vorbis (WebGL)
- Quality: 70-80%
- Load Type: 
  - Compressed in Memory (музыка)
  - Decompress on Load (короткие SFX)
  - Streaming (длинные треки)

---

### 📐 Технические ограничения для Mobile WebGL

#### **Performance Target**

**Target FPS**: 30-60 fps (адаптивно)
- High-end devices: 60 fps
- Mid-range: 30-45 fps
- Low-end: 30 fps (минимум)

**Memory Budget**:
- Total RAM: 256 MB - 512 MB MAX (WebGL setting)
- Texture Memory: 100-150 MB
- Audio Memory: 20-30 MB
- Scene Memory: 50-70 MB

**Draw Calls**: 
- Target: до 50 draw calls на сцену
- UI batching через Canvas
- Sprite atlasing

**Texture Limits**:
- Max texture size: 2048x2048
- Compression: ASTC / ETC2 (Android), PVRTC (iOS)
- Mipmaps: Enabled для 3D, disabled для UI

#### **Loading Times**

**Целевые показатели**:
- Initial Load: до 10 секунд (хорошее соединение)
- Scene Transition: до 1 секунды
- Addressables Load: до 2-3 секунд

**Оптимизации**:
- Asynchronous scene loading
- Progress bar для длительных загрузок
- Lazy loading для некритичного контента

#### **Network**

**API Response Time**: до 500ms
- Critical requests (tap): до 100ms
- Leaderboard: до 1 sec
- Inventory load: до 500ms

**Offline Support**:
- Кэширование последнего состояния игры
- Queue для offline тапов (sync при reconnect)
- Grace period для оффлайн бонусов

---

## 🔗 Unity ↔ React ↔ Telegram интеграция

### Общая концепция Bridge

**Unity WebGL** взаимодействует с **React** через **jslib** плагины и **JavaScript Interop**.  
**React** взаимодействует с **Telegram Web App SDK** через официальный API.

### Ключевые функции Bridge

**Unity → JavaScript**:
- Вызов Telegram функций (share, haptic, navigation)
- Открытие внешних ссылок
- Подключение кошельков
- Аналитические события

**JavaScript → Unity**:
- Передача Telegram initData для аутентификации
- Уведомления о событиях (wallet connected, back button pressed)
- Результаты внешних операций

**Telegram Web App SDK функции**:
- `Telegram.WebApp.initData` - данные для аутентификации
- `Telegram.WebApp.BackButton` - системная кноп��а назад
- `Telegram.WebApp.MainButton` - главная кнопка
- `Telegram.WebApp.HapticFeedback` - вибрация
- `Telegram.WebApp.openLink()` - открытие ссылок
- `Telegram.WebApp.shareUrl()` - шаринг

---

## 📊 Аналитика

### События для трекинга

**Unity Analytics + Custom Backend Events**:

1. **User Events**:
   - Registration (from Telegram)
   - Login
   - Session Start/End
   - Playtime

2. **Gameplay Events**:
   - Taps Count
   - Coins Collected (from taps)
   - Coins Collected (from welcome bonus)
   - Coins Collected (from chests)
   - Level Up (уровень игрока)
   - Items Collected
   - Items Merged
   - Items Equipped
   - Upgrades Purchased

3. **Social Events**:
   - Friends Invited
   - Referral Rewards Claimed
   - Tournament Joined
   - Tournament Rank

4. **Monetization Events**:
   - Chest Purchased
   - Booster Purchased
   - Booster Activated
   - Wallet Connected
   - Deposit Made
   - Withdrawal Made

5. **Retention Metrics**:
   - Day 1, 3, 7, 14, 30 Retention
   - Sessions per User
   - Average Session Length
   - Churn Rate

**Инструменты**:
- Google Analytics / Firebase
- Custom Backend Logging
- Telegram Bot Analytics

---

## 🌐 Локализация

**Языки**: EN (English), RU (Russian)

**Unity Localization Package**:
- String tables (JSON)
- Asset tables (текстуры с текстом, если нужно)
- Smart Strings (форматирование чисел, валют)
- Fallback: EN (если перевода нет)

**Переводимые элементы**:
- Все UI тексты
- Tutorial тексты
- Описания предметов
- Названия достижений/квестов
- Уведомления

**Загрузка**:
- Через Addressables (динамическая смена языка)
- Выбор языка в Settings

---

## ⚙️ Сервер: REST API настройки

### Node.js + Express.js конфигурация

**Основные middleware**:

```javascript
// CORS для WebGL
app.use(cors({
  origin: [
    'https://game.wattstap.com',
    'https://cdn.wattstap.com',
    'https://t.me'
  ],
  credentials: true,
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization', 'X-Telegram-Init-Data']
}));

// Security
app.use(helmet());
app.use(express.json({ limit: '10mb' }));

// Rate Limiting
const limiter = rateLimit({
  windowMs: 1 * 60 * 1000, // 1 минута
  max: 100 // 100 запросов
});
app.use('/api/', limiter);

// Telegram Data Validation
app.use('/api/auth', telegramAuthMiddleware);

// JWT Verification
app.use('/api/*', jwtMiddleware);
```

**Error Handling**:
- Стандартизированные коды ошибок
- JSON response format: `{ success: bool, data: {}, error: string }`

---

## 📱 Оптимизация для мобильного WebGL в Telegram

### Критически важные настройки Unity

**Unity Player Settings (WebGL)**:

```
Resolution and Presentation:
  - Default Canvas Width: 720
  - Default Canvas Height: 1280
  - Run In Background: Enabled
  
Publishing Settings:
  - Compression Format: Brotli
  - Enable Exceptions: None
  - Data Caching: Enabled
  - Debug Symbols: Disabled (production)
  
Memory:
  - Memory Size: 256 MB
  - Auto Graphics API: Enabled (WebGL 2.0 fallback to 1.0)
  
Code Optimization:
  - IL2CPP Code Generation: Faster runtime
  - Managed Stripping Level: High
  - Strip Engine Code: Enabled
  
Other:
  - Multithreaded Rendering: Disabled (WebGL не поддерживает)
  - GPU Skinning: Disabled (экономия памяти)
```

### Telegram WebView специфика

**iOS Telegram WebView**:
- Ограничение памяти: 256 MB
- Нет поддержки IndexedDB в старых версиях
- Используем LocalStorage для критичных данных

**Android Telegram WebView**:
- Лучшая производительность
- Поддержка WebGL 2.0
- Меньше ограничений по памяти

### Оптимизации

1. **Ленивая загрузка** (Addressables):
   - Загружать экраны по требованию
   - Разгружать неактивные ассеты
   - Preload критичные ассеты

2. **Quality Settings**:
   - Shadow Quality: Disabled
   - Anti-Aliasing: None (или FXAA low)
   - Texture Quality: Medium
   - VSync Count: Don't Sync (для контроля FPS)

3. **Texture Streaming**:
   - Включить для больших текстур
   - Mipmaps для динамически загружаемых текстур

4. **Object Pooling**:
   - Для часто создаваемых объектов (coins, particles)
   - Избежать Garbage Collection spikes

5. **Батчинг**:
   - Static Batching для статичного UI
   - Dynamic Batching для мелких объектов
   - SRP Batcher (если используем URP)

---

## ✅ Чек-лист стабильности

### Обязательные тесты перед релизом

**Telegram WebView (iOS)**:
- [ ] Стабильные 30 fps на iPhone 11
- [ ] Нет крашей при переключении приложений
- [ ] Корректная работа на разных разрешениях

**Telegram WebView (Android)**:
- [ ] 30-60 fps на mid-range устройствах
- [ ] Нет утечек памяти (игра > 30 минут)
- [ ] Корректная работа на разных разрешениях
- [ ] WebGL 2.0 fallback на WebGL 1.0

**Десктоп (Telegram Desktop / Web)**:
- [ ] Полная функциональность
- [ ] 60 fps
- [ ] Keyboard input (если нужно)
- [ ] Mouse/Touch input

**Backend**:
- [ ] Все API endpoints с < 500ms response time
- [ ] Rate limiting работает
- [ ] Anti-cheat валидация
- [ ] HTTPS на всех endpoints
- [ ] CORS настроен корректно
- [ ] JWT токены валидируются

**Аутентификация**:
- [ ] Telegram initData корректно валидируется
- [ ] Защита от replay-атак
- [ ] Session expiration работает

**CDN & Addressables**:
- [ ] Все бандлы доступны через HTTPS
- [ ] Кэширование работает
- [ ] Fallback на старые версии при ошибке загрузки

**CI/CD**:
- [ ] Автоматический deploy работает
- [ ] Rollback функция тестирована
- [ ] Staging окружение идентично production
- [ ] SSL сертификаты автоматически обновляются

---

## 📝 Примечания

**Unity 6 преимущества**:
- Улучшенная производительность WebGL
- Лучшая поддержка мобильных браузеров
- Оптимизированная загру��ка
- Встроенный Addressables Package

**Важные детали**:
- Все коммуникации только HTTPS
- Server-authoritative логика (защита от читов)
- Graceful degradation (игра работает даже на слабых устройствах)
- Progressive loading (пользователь может играть пока догружается контент)

**Мониторинг**:
- Server health checks каждые 5 минут
- Error tracking (Sentry / Rollbar)
- Performance monitoring (Unity Analytics)
- User feedback форма в Settings

---

# WattsTap - План Разработки

## Текущее Состояние Проекта

### ✅ Реализовано
1. **Базовая Архитектура**
   - Service Locator для управления сервисами
   - Система конфигураций (ConfigService)
   - UI сервис с поддержкой открытия/закрытия/показа/скрытия экранов
   - MVP паттерн для UI (View-Presenter)
   - ApplicationEntry - точка входа в приложение
   - Базовые интерфейсы (IService, IConfig, IUIView, IUIViewPresenter)

2. **UI Система**
   - UIBaseView и UIBasePresenter
   - UIHost для управления слоями UI
   - UIConfig для хранения префабов UI
   - MainMenuUIView/Presenter (базовая реализация)

3. **Константы**
   - UIConstants
   - ConfigsConstants

### ❌ Отсутствует

#### Критическая функциональность
1. **Игровая Механика**
   - Tap система (основная механика игры)
   - Система энергии/ресурсов
   - Система прогрессии игрока
   - Система улучшений (upgrades)
   - Система достижений
   - Система квестов/задач

2. **Данные и Персистентность**
   - Система сохранения/загрузки данных
   - Модели данных игрока
   - Система аналитики
   - Облачное сохранение

3. **Backend Интеграция**
   - HTTP клиент для API запросов
   - WebSocket для реального времени
   - Система аутентификации
   - Обработка ошибок сети

4. **UI Экраны**
   - Главное меню (полная версия)
   - Игровой экран
   - Магазин
   - Экран улучшений
   - Экран настроек
   - Экран достижений
   - Экран лидерборда
   - Всплывающие окна (попапы)

5. **Визуальные Эффекты**
   - Particle системы
   - Анимации UI
   - Feedback системы (тактильный, визуальный, звуковой)
   - Система спрайтов и атласов

6. **Аудио**
   - Аудио менеджер
   - Система звуковых эффектов
   - Фоновая музыка
   - Настройки звука

7. **Монетизация**
   - Система покупок (IAP)
   - Рекламная интеграция
   - Система валют (мягкая/жесткая валюта)

8. **Социальные Функции**
   - Система друзей
   - Чат
   - Кланы/Гильдии
   - Лидерборды

9. **Оптимизация и Утилиты**
   - Object pooling
   - Система событий (EventBus)
   - Локализация
   - Система логирования
   - DevTools/Debug меню

---

## План Разработки (2-недельные спринты с запасом x1.5)

### Спринт 1: Основа Игровой Механики (3 недели)
**Период:** Недели 1-3

#### Задачи:
1. **Система Игровых Данных** (5 дней)
   - [ ] Создать модели данных игрока (PlayerData, PlayerStats)
   - [ ] Реализовать систему ресурсов (энергия, монеты, кристаллы)
   - [ ] Создать GameDataService для управления игровыми данными
   - [ ] Добавить систему событий для изменения данных
   - [ ] Unit тесты для моделей данных

2. **Система Сохранения** (4 дня)
   - [ ] Реализовать ISaveService интерфейс
   - [ ] LocalSaveService с использованием PlayerPrefs/JSON
   - [ ] Сериализация/десериализация данных
   - [ ] Система версионирования сохранений
   - [ ] Обработка поврежденных сохранений

3. **Базовая Tap Механика** (6 дней)
   - [ ] TapController для обработки кликов
   - [ ] Визуализация клика (эффекты, анимация)
   - [ ] Начисление ресурсов за клик
   - [ ] Система множителей
   - [ ] Feedback система (визуальный/тактильный)
   - [ ] Оптимизация обработки множественных кликов

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 2: Система Улучшений и UI (3 недели)
**Период:** Недели 4-6

#### Задачи:
1. **Система Улучшений** (6 дней)
   - [ ] Модель данных улучшений (UpgradeData, UpgradeConfig)
   - [ ] UpgradeService для управления улучшениями
   - [ ] Система расчета стоимости улучшений
   - [ ] Система требований для разблокировки
   - [ ] Балансировка улучшений (конфиги)

2. **UI Экраны - Часть 1** (7 дней)
   - [ ] Переработать MainMenu (кнопки, навигация)
   - [ ] Создать GameScreen (основной игровой экран)
   - [ ] HUD с отображением ресурсов
   - [ ] Экран улучшений (UpgradesView/Presenter)
   - [ ] Компоненты UI (кнопки улучшений, прогресс бары)
   - [ ] Анимации переходов между экранами

3. **Система Событий** (2 дня)
   - [ ] Реализовать EventBus
   - [ ] Создать GameEvents (ресурсы, улучшения, достижения)
   - [ ] Интегрировать с существующими системами

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 3: Визуальные Эффекты �� Анимации (3 недели)
**Период:** Недели 7-9

#### Задачи:
1. **VFX Система** (5 дней)
   - [ ] Particle системы для кликов
   - [ ] Эффекты получения ресурсов
   - [ ] Эффекты улучшений
   - [ ] Object pooling для частиц
   - [ ] VFX конфиги

2. **Анимации UI** (5 дней)
   - [ ] DOTween интеграция
   - [ ] Анимации появления/исчезновения панелей
   - [ ] Анимации кнопок (hover, press, disabled)
   - [ ] Анимации счетчиков (числа)
   - [ ] Transition система между экранами

3. **Feedback Системы** (5 дней)
   - [ ] Haptic feedback менеджер
   - [ ] Screen shake эффекты
   - [ ] Visual feedback (вспышки, glow)
   - [ ] Система комбо (multiple taps)
   - [ ] Juice эффекты для улучшений

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 4: Аудио и Настройки (3 недели)
**Период:** Недели 10-12

#### Задачи:
1. **Audio Система** (6 дней)
   - [ ] AudioService для управления звуком
   - [ ] Система звуковых эффектов (SFX)
   - [ ] Музыкальный менеджер (фоновая музыка)
   - [ ] Audio pooling для SFX
   - [ ] AudioConfig для настройки звуков
   - [ ] Плавные переходы музыки

2. **Экран Настроек** (4 дня)
   - [ ] SettingsView/Presenter
   - [ ] Настройки звука (музыка, SFX)
   - [ ] Настройки графики
   - [ ] Настройки вибрации
   - [ ] Сохранение настроек
   - [ ] UI компоненты (слайдеры, переключатели)

3. **Система Достижений** (5 дней)
   - [ ] Модель данных достижений
   - [ ] AchievementService
   - [ ] Трекинг прогресса достижений
   - [ ] UI уведомления о достижениях
   - [ ] Экран достижений (AchievementsView)
   - [ ] Конфиги достижений

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 5: Backend Интеграция - Часть 1 (3 недели)
**Период:** Недели 13-15

#### Задачи:
1. **HTTP Клиент** (5 дней)
   - [ ] HttpService с UnityWebRequest
   - [ ] Система обработки запросов/ответов
   - [ ] Обработка ошибок и retry логика
   - [ ] Request/Response модели
   - [ ] API endpoint константы

2. **Аутентификация** (5 дней)
   - [ ] Система регистрации/входа
   - [ ] JWT token управление
   - [ ] Автоматическое обновление токенов
   - [ ] Связь с device ID
   - [ ] Безопасное хранение токенов

3. **Синхронизация Данных** (5 дней)
   - [ ] Загрузка данных игрока с сервера
   - [ ] Сохранение прогресса на сервер
   - [ ] Conflict resolution (локальные vs серверные данные)
   - [ ] Offline режим
   - [ ] Очередь запросов

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 6: Backend Интеграция - Часть 2 (3 недели)
**Период:** Недели 16-18

#### Задачи:
1. **WebSocket Интеграция** (6 дней)
   - [ ] WebSocketService
   - [ ] Подключение/переподключение
   - [ ] Обработка real-time событий
   - [ ] Heartbeat система
   - [ ] Обработка разрыва соединения

2. **Лидерборды** (5 дней)
   - [ ] LeaderboardService
   - [ ] Загрузка топов (глобальный, друзья)
   - [ ] LeaderboardView/Presenter
   - [ ] Обновление в реальном времени
   - [ ] Кэширование данных лидерборда

3. **Система Квестов** (4 дня)
   - [ ] Модель данных квестов
   - [ ] QuestService
   - [ ] Трекинг прогресса квестов
   - [ ] UI уведомления о квестах
   - [ ] Экран квестов (QuestsView)

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 7: Монетизация (3 недели)
**Период:** Недели 19-21

#### Задачи:
1. **Система Валют** (3 дня)
   - [ ] Модели валют (мягкая, жесткая)
   - [ ] CurrencyService
   - [ ] Транзакции и валидация
   - [ ] UI отображение валют
   - [ ] История транзакций

2. **In-App Purchases** (7 дней)
   - [ ] Unity IAP интеграция
   - [ ] IAPService
   - [ ] Продуктовый каталог
   - [ ] Обработка покупок
   - [ ] Восстановление покупок
   - [ ] Валидация покупок на сервере
   - [ ] UI магазина

3. **Реклама** (5 дней)
   - [ ] Интеграция SDK (Unity Ads / AdMob)
   - [ ] AdService
   - [ ] Rewarded видео
   - [ ] Interstitial реклама
   - [ ] Banner реклама
   - [ ] Система наград за рекламу

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 8: Социальные Функции (3 недели)
**Период:** Недели 22-24

#### Задачи:
1. **Система Друзей** (6 дней)
   - [ ] FriendsService
   - [ ] Поиск друзей
   - [ ] Приглашения/принятие
   - [ ] Список друзей
   - [ ] FriendsView/Presenter
   - [ ] Подарки друзьям

2. **Система Кланов** (6 дней)
   - [ ] ClanService
   - [ ] Создание/вступление в клан
   - [ ] Управление кланом
   - [ ] Клановые задания
   - [ ] ClanView/Presenter
   - [ ] Клановый чат

3. **Базовый Чат** (3 дня)
   - [ ] ChatService
   - [ ] Отправка/получение сообщений
   - [ ] ChatView/Presenter
   - [ ] Фильтры/модерация

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 9: Оптимизация и Полировка (3 недели)
**Период:** Недели 25-27

#### Задачи:
1. **Object Pooling** (4 дня)
   - [ ] Generic ObjectPool система
   - [ ] Pooling для UI элементов
   - [ ] Pooling для VFX
   - [ ] Pooling для game objects
   - [ ] Performance тесты

2. **Локализация** (5 дней)
   - [ ] LocalizationService
   - [ ] Загрузка языковых файлов
   - [ ] UI локализация
   - [ ] Переключение языка в runtime
   - [ ] Fallback на английский
   - [ ] Минимум 2-3 языка (EN, RU, + 1)

3. **Debug Tools** (3 дня)
   - [ ] Debug menu
   - [ ] Читы для тестирования
   - [ ] Логирование (уровни, фильтры)
   - [ ] Performance мониторинг
   - [ ] Analytics debug view

4. **Performance Optimization** (3 дня)
   - [ ] Профилирование узких мест
   - [ ] Оптимизация UI draw calls
   - [ ] Memory профилирование
   - [ ] Оптимизация загрузки
   - [ ] Build size оптимизация

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 10: Аналитика и Тестирование (3 недели)
**Период:** Недели 28-30

#### Задачи:
1. **Система Аналитики** (5 дней)
   - [ ] AnalyticsService
   - [ ] Интеграция Firebase Analytics
   - [ ] Кастомные события
   - [ ] Трекинг конверсии
   - [ ] Funnel аналитика
   - [ ] User properties

2. **Tutorialная Система** (5 дней)
   - [ ] TutorialService
   - [ ] Step-by-step гайды
   - [ ] Highlight система
   - [ ] Прогресс туториала
   - [ ] Скипаемые туториалы

3. **QA и Тестирование** (5 дней)
   - [ ] Unit тесты для критических систем
   - [ ] Integration тесты
   - [ ] UI тесты
   - [ ] Баг фиксы
   - [ ] Stability улучшения

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 11: Advanced Features (3 недели)
**Период:** Недели 31-33

#### Задачи:
1. **Система Событий (Events)** (5 дней)
   - [ ] Limited-time события
   - [ ] EventService
   - [ ] Специальные награды
   - [ ] UI таймеры событий
   - [ ] Конфиги событий

2. **Daily Rewards** (3 дня)
   - [ ] DailyRewardService
   - [ ] Календарь наград
   - [ ] Streak система
   - [ ] DailyRewardView

3. **Battle Pass / Season Pass** (7 дней)
   - [ ] SeasonPassService
   - [ ] Уровни и награды
   - [ ] Free vs Premium треки
   - [ ] SeasonPassView/Presenter
   - [ ] Progress трекинг

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

### Спринт 12: Финальная Полировка и Релиз (3 недели)
**Период:** Недели 34-36

#### Задачи:
1. **UI/UX Полировка** (5 дней)
   - [ ] Финальный дизайн всех экранов
   - [ ] Консистентность UI
   - [ ] Accessibility улучшения
   - [ ] Onboarding flow
   - [ ] User feedback интеграция

2. **Performance Final Pass** (4 дня)
   - [ ] Финальная оптимизация
   - [ ] Load time optimization
   - [ ] Memory leak fixes
   - [ ] Battery consumption tests
   - [ ] Low-end device тестирование

3. **Release Preparation** (6 дней)
   - [ ] Store materials (скриншоты, описание)
   - [ ] Privacy policy
   - [ ] Terms of service
   - [ ] Build pipeline настройка
   - [ ] Soft launch подготовка
   - [ ] Мониторинг и alerting setup

**Итого:** 15 дней → 22.5 дня (3 недели с запасом x1.5)

---

## Общая Сводка

### Временные Рамки
- **Общий срок разработки:** 36 недель (9 месяцев)
- **Количество спринтов:** 12 спринтов по 3 недели
- **Базовое время без запаса:** 24 недели (6 месяцев)
- **Запас времени:** x1.5 коэффициент

### Приоритеты

#### P0 (Критические для MVP)
- Спринты 1-4: Игровая механика, UI, визуалы, аудио
- **Срок для MVP:** 12 недель (3 месяца)

#### P1 (Важные для релиза)
- Спринты 5-8: Backend, монетизация, социальные функции
- **Срок для Soft Launch:** 24 недели (6 месяцев)

#### P2 (Nice to have)
- Спринты 9-12: Оптимизация, advanced features, полировка
- **Срок для Full Launch:** 36 недель (9 месяцев)

### Метрики Успеха
- [ ] Core loop работает и enjoyable
- [ ] Retention D1 > 40%, D7 > 20%
- [ ] Monetization ARPU > $0.50
- [ ] Crash rate < 1%
- [ ] Load time < 3 сек
- [ ] 4+ звезд в сторах

### Риски и Митигация

#### Технические Риски
1. **Backend интеграция сложнее ожидаемого**
   - Митигация: Начать интеграцию раньше, заложить больше времени

2. **Performance на слабых устройствах**
   - Митигация: Тестировать на low-end с самого начала

3. **Сложности с монетизацией SDK**
   - Митигация: Изучить документацию заранее, тестовая интеграция

#### Бизнес Риски
1. **Изменения требований**
   - Митигация: Гибкая архитектура, модульность

2. **Нехватка ресурсов**
   - Митигация: Приоритизация MVP, откладывание nice-to-have

### Рекомендации
1. **Начать с MVP** (Спринты 1-4) для быстрой валидации core loop
2. **Регулярные playtests** начиная со спринта 2
3. **Метрики и аналитику** внедрить как можно раньше
4. **Code reviews** и архитектурные ревью каждый спринт
5. **Документация** обновляется параллельно с разработкой
