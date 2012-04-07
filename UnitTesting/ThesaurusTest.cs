using watch_assistant.Model.Dictionary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UnitTesting
{
    
    
    /// <summary>
    ///Это класс теста для ThesaurusTest, в котором должны
    ///находиться все модульные тесты ThesaurusTest
    ///</summary>
    [TestClass()]
    public class ThesaurusTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты теста
        // 
        //При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Тест для Конструктор Thesaurus
        ///</summary>
        [TestMethod()]
        public void ThesaurusConstructorTest()
        {
            Thesaurus target = new Thesaurus();
            Assert.IsTrue(!String.IsNullOrEmpty(target.Name));
            Console.WriteLine(target.Name);
        }

        /// <summary>
        ///Тест для AddDefinition
        ///</summary>
        [TestMethod()]
        public void AddDefinitionTest()
        {
            Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
            target.AddDefinition("Красный", "Красивый", true);
            target.AddDefinition("Красный", "Красивый", true);
            target.AddDefinition("Красивый", "Красный", false);
            target.AddDefinition("Красивый", "Красный", true);
            Assert.IsTrue(target.Count == 2);
            target.AddDefinition("Кровавый", "Красный", true);
            Assert.IsTrue(target.Count == 3);
        }

        /// <summary>
        ///Тест для AddDefinition
        ///</summary>
        [TestMethod()]
        public void AddDefinitionTest1()
        {
            Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
            target.AddDefinition("Красный", new string[] { "Кровавый", "Красивый" }, true);
            target.AddDefinition("Красный", new string[] { "Кровавый", "Красивый" }, true);
            Assert.IsTrue(target.Count == 3);
        }

        ///// <summary>
        /////Тест для GetObjectData
        /////</summary>
        //[TestMethod()]
        //public void GetObjectDataTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    SerializationInfo info = null; // TODO: инициализация подходящего значения
        //    StreamingContext context = new StreamingContext(); // TODO: инициализация подходящего значения
        //    target.GetObjectData(info, context);
        //    Assert.Inconclusive("Невозможно проверить метод, не возвращающий значение.");
        //}

        ///// <summary>
        /////Тест для OnDeserialization
        /////</summary>
        //[TestMethod()]
        //public void OnDeserializationTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    object sender = null; // TODO: инициализация подходящего значения
        //    target.OnDeserialization(sender);
        //    Assert.Inconclusive("Невозможно проверить метод, не возвращающий значение.");
        //}

        /// <summary>
        ///Тест для RemoveDefinition
        ///</summary>
        [TestMethod()]
        public void RemoveDefinitionTest()
        {
            Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
            target.AddDefinition("Красный", new string[] { "Кровавый", "Красивый" }, true);
            bool expected = true; // TODO: инициализация подходящего значения
            bool actual = target.RemoveDefinition("Кровавый", true);
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(target.Count == 2);
        }

        ///// <summary>
        /////Тест для RemoveDefinition
        /////</summary>
        //[TestMethod()]
        //public void RemoveDefinitionTest1()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string key = string.Empty; // TODO: инициализация подходящего значения
        //    string meaning = string.Empty; // TODO: инициализация подходящего значения
        //    bool mutual = false; // TODO: инициализация подходящего значения
        //    bool expected = false; // TODO: инициализация подходящего значения
        //    bool actual;
        //    actual = target.RemoveDefinition(key, meaning, mutual);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Проверьте правильность этого метода теста.");
        //}

        ///// <summary>
        /////Тест для SetDefinition
        /////</summary>
        //[TestMethod()]
        //public void SetDefinitionTest()
        //{
        //    Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
        //    string key = string.Empty; // TODO: инициализация подходящего значения
        //    IEnumerable<string> definition = null; // TODO: инициализация подходящего значения
        //    bool mutual = false; // TODO: инициализация подходящего значения
        //    target.SetDefinition(key, definition, mutual);
        //    Assert.Inconclusive("Невозможно проверить метод, не возвращающий значение.");
        //}
    }
}
