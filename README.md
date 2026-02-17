# Homework-Collector-bots

Технологии: Unity, DOTween, NavMesh, корутины, ScriptableObject.

Архитектура: событийная шина с фильтрацией [IntelligentEventAggregator](https://github.com/JunDmitry/Homework-Collector-bots/blob/main/Assets/Scripts/Infrastructure/EventManagement/IntelligentEventAggregator.cs), система поведений [IBaseBehaviour](https://github.com/JunDmitry/Homework-Collector-bots/blob/main/Assets/Scripts/Gameplay/Features/Behaviour/Contracts/IBaseBehaviour.cs), DI (ручной), репозитории, MVP для UI.

Системы: очередь задач с приоритетами, распределение заданий между рабочими, строительство зданий, пулы ресурсов.

Паттерны: Command, Composite, Decorator, Mediator, Null Object, Template Method, Value Object.
