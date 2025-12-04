// ============================================
// CombatTestSceneGenerator.cs
// Editor tool to generate combat test scene
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;

namespace HNR.Editor
{
    public static class CombatTestSceneGenerator
    {
        [MenuItem("HNR/Generate Combat Test Scene")]
        public static void GenerateCombatTestScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // === Create Managers Parent ===
            GameObject managersParent = new GameObject("--- MANAGERS ---");

            // === TurnManager ===
            GameObject turnManagerObj = new GameObject("TurnManager");
            turnManagerObj.transform.SetParent(managersParent.transform);
            turnManagerObj.AddComponent<Combat.TurnManager>();

            // === DeckManager ===
            GameObject deckManagerObj = new GameObject("DeckManager");
            deckManagerObj.transform.SetParent(managersParent.transform);
            deckManagerObj.AddComponent<Combat.DeckManager>();

            // === HandManager ===
            GameObject handManagerObj = new GameObject("HandManager");
            handManagerObj.transform.SetParent(managersParent.transform);
            var handManager = handManagerObj.AddComponent<Combat.HandManager>();

            // === CardExecutor (Week 4) ===
            GameObject cardExecutorObj = new GameObject("CardExecutor");
            cardExecutorObj.transform.SetParent(managersParent.transform);
            cardExecutorObj.AddComponent<Cards.CardExecutor>();

            // === TargetingSystem (Week 4) ===
            GameObject targetingSystemObj = new GameObject("TargetingSystem");
            targetingSystemObj.transform.SetParent(managersParent.transform);
            targetingSystemObj.AddComponent<Combat.TargetingSystem>();

            // === Create UI Canvas ===
            GameObject canvasObj = new GameObject("CombatCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // === CombatScreen ===
            GameObject combatScreenObj = new GameObject("CombatScreen");
            combatScreenObj.transform.SetParent(canvasObj.transform, false);
            RectTransform screenRect = combatScreenObj.AddComponent<RectTransform>();
            screenRect.anchorMin = Vector2.zero;
            screenRect.anchorMax = Vector2.one;
            screenRect.sizeDelta = Vector2.zero;
            var combatScreen = combatScreenObj.AddComponent<UI.CombatScreen>();

            // === Top Bar (AP, HP, Block) ===
            GameObject topBar = CreatePanel(combatScreenObj, "TopBar", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -50), new Vector2(0, 100));

            // AP Text
            GameObject apObj = CreateText(topBar, "APText", "3/3", 32);
            RectTransform apRect = apObj.GetComponent<RectTransform>();
            apRect.anchorMin = new Vector2(0, 0.5f);
            apRect.anchorMax = new Vector2(0, 0.5f);
            apRect.anchoredPosition = new Vector2(100, 0);

            // HP Slider
            GameObject hpSliderObj = CreateSlider(topBar, "HPSlider");
            RectTransform hpRect = hpSliderObj.GetComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(0.5f, 0.5f);
            hpRect.anchorMax = new Vector2(0.5f, 0.5f);
            hpRect.anchoredPosition = Vector2.zero;
            hpRect.sizeDelta = new Vector2(300, 30);

            // HP Text
            GameObject hpTextObj = CreateText(topBar, "HPText", "100/100", 24);
            RectTransform hpTextRect = hpTextObj.GetComponent<RectTransform>();
            hpTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            hpTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            hpTextRect.anchoredPosition = new Vector2(0, -30);

            // Block Text
            GameObject blockObj = CreateText(topBar, "BlockText", "", 28);
            RectTransform blockRect = blockObj.GetComponent<RectTransform>();
            blockRect.anchorMin = new Vector2(1, 0.5f);
            blockRect.anchorMax = new Vector2(1, 0.5f);
            blockRect.anchoredPosition = new Vector2(-100, 0);

            // === Bottom Bar (Deck Info, End Turn) ===
            GameObject bottomBar = CreatePanel(combatScreenObj, "BottomBar", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 50), new Vector2(0, 100));

            // Draw Pile
            GameObject drawObj = CreateText(bottomBar, "DrawPileText", "0", 24);
            RectTransform drawRect = drawObj.GetComponent<RectTransform>();
            drawRect.anchorMin = new Vector2(0, 0.5f);
            drawRect.anchorMax = new Vector2(0, 0.5f);
            drawRect.anchoredPosition = new Vector2(80, 0);

            // Discard Pile
            GameObject discardObj = CreateText(bottomBar, "DiscardPileText", "0", 24);
            RectTransform discardRect = discardObj.GetComponent<RectTransform>();
            discardRect.anchorMin = new Vector2(1, 0.5f);
            discardRect.anchorMax = new Vector2(1, 0.5f);
            discardRect.anchoredPosition = new Vector2(-80, 0);

            // End Turn Button
            GameObject endTurnObj = CreateButton(bottomBar, "EndTurnButton", "End Turn");
            RectTransform endTurnRect = endTurnObj.GetComponent<RectTransform>();
            endTurnRect.anchorMin = new Vector2(0.5f, 0.5f);
            endTurnRect.anchorMax = new Vector2(0.5f, 0.5f);
            endTurnRect.anchoredPosition = Vector2.zero;
            endTurnRect.sizeDelta = new Vector2(150, 50);

            // === Turn Indicator ===
            GameObject turnObj = CreateText(combatScreenObj, "TurnText", "Turn 1", 28);
            RectTransform turnRect = turnObj.GetComponent<RectTransform>();
            turnRect.anchorMin = new Vector2(0.5f, 1);
            turnRect.anchorMax = new Vector2(0.5f, 1);
            turnRect.anchoredPosition = new Vector2(0, -120);

            // === Hand Container ===
            GameObject handContainer = new GameObject("HandContainer");
            handContainer.transform.SetParent(canvasObj.transform, false);
            RectTransform handRect = handContainer.AddComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.5f, 0);
            handRect.anchorMax = new Vector2(0.5f, 0);
            handRect.anchoredPosition = new Vector2(0, 200);
            handRect.sizeDelta = new Vector2(800, 300);

            // === Test Managers ===
            GameObject testManagerObj = new GameObject("CombatTestManager");
            testManagerObj.AddComponent<Testing.CombatTestManager>();

            GameObject week4TestObj = new GameObject("Week4IntegrationTest");
            week4TestObj.AddComponent<Testing.Week4IntegrationTest>();

            // === DamageNumberSpawner (Week 4) ===
            GameObject damageSpawnerObj = new GameObject("DamageNumberSpawner");
            damageSpawnerObj.transform.SetParent(canvasObj.transform, false);
            damageSpawnerObj.AddComponent<UI.DamageNumberSpawner>();

            // === Enemy Container ===
            GameObject enemyContainer = new GameObject("EnemyContainer");
            enemyContainer.transform.SetParent(canvasObj.transform, false);
            RectTransform enemyRect = enemyContainer.AddComponent<RectTransform>();
            enemyRect.anchorMin = new Vector2(0.5f, 0.6f);
            enemyRect.anchorMax = new Vector2(0.5f, 0.6f);
            enemyRect.anchoredPosition = Vector2.zero;
            enemyRect.sizeDelta = new Vector2(600, 200);

            // === Wire References ===
            SerializedObject screenSO = new SerializedObject(combatScreen);
            screenSO.FindProperty("_apText").objectReferenceValue = apObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_hpSlider").objectReferenceValue = hpSliderObj.GetComponent<Slider>();
            screenSO.FindProperty("_hpText").objectReferenceValue = hpTextObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_blockText").objectReferenceValue = blockObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_drawPileText").objectReferenceValue = drawObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_discardPileText").objectReferenceValue = discardObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_endTurnButton").objectReferenceValue = endTurnObj.GetComponent<Button>();
            screenSO.FindProperty("_turnText").objectReferenceValue = turnObj.GetComponent<TextMeshProUGUI>();
            screenSO.ApplyModifiedPropertiesWithoutUndo();

            // Wire HandManager container
            SerializedObject handSO = new SerializedObject(handManager);
            handSO.FindProperty("_handContainer").objectReferenceValue = handContainer.transform;
            handSO.ApplyModifiedPropertiesWithoutUndo();

            // Save scene
            string scenePath = "Assets/_Project/Scenes/CombatTest.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[CombatTestSceneGenerator] Created combat test scene at {scenePath}");
            Debug.Log("Controls: T = Start Combat/Run Tests, Y = Test Card Play, Space = End Turn, D = Deal Damage, B = Add Block, K = Kill Enemy");
            Debug.Log("Week 4 Components: CardExecutor, TargetingSystem, DamageNumberSpawner, Week4IntegrationTest");
        }

        private static GameObject CreatePanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.5f);
            return obj;
        }

        private static GameObject CreateText(GameObject parent, string name, string text, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            obj.AddComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return obj;
        }

        private static GameObject CreateSlider(GameObject parent, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            obj.AddComponent<RectTransform>();
            Slider slider = obj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(obj.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(obj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.8f, 0.2f, 0.2f);

            slider.fillRect = fillRect;

            return obj;
        }

        private static GameObject CreateButton(GameObject parent, string name, string text)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            obj.AddComponent<RectTransform>();
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.6f);
            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            GameObject textObj = CreateText(obj, "Text", text, 20);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return obj;
        }
    }
}
