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
            target.AddDefinition("Красный", "Кровавый", true);
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

        /// <summary>
        ///Тест для GetPhraseVariations
        ///</summary>
        [TestMethod()]
        public void GetPhraseVariationsTest()
        {
            Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
            target.AddDefinition("красный", new string[] { "кровавый", "красивый" }, true);
            target.AddDefinition("вертолет", new string[] { "геликоптер", "хели" }, true);
            target.AddDefinition("быстрый", new string[] { "молниеносный", "скоростной" },true);
            string[] phrases = target.GetPhrasePermutations("Огромный чебурек");
            Assert.IsTrue(phrases.Length > 0);
        }

        /// <summary>
        ///Тест для Serialize/Deserialize
        ///</summary>
        [TestMethod()]
        public void SerializeDeserializeTest()
        {
            Thesaurus target = new Thesaurus(); // TODO: инициализация подходящего значения
            target.AddDefinition("красный", new string[] { "кровавый", "красивый" }, true);
            target.AddDefinition("вертолет", new string[] { "геликоптер", "хели" }, true);
            target.AddDefinition("быстрый", new string[] { "молниеносный", "скоростной" }, true);
            target.Serialize(target.Name);
            Thesaurus deserialized = new Thesaurus(target.Name);
            Assert.AreEqual(target.Count, deserialized.Count);
        }
    }
}
