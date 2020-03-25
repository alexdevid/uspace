﻿using System;
using System.Collections.Generic;
using Generator;
using Model;
using UI.General;
using UI.SinglePlayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityPackages;

namespace Scene.SinglePlayer
{
    public class WorldSelect : SceneController
    {
        public Button createButton;
        public Button backButton;
        public Button playButton;
        public Button deleteButton;
        public Button showCreateOverlayButton;
        public Button hideCreateOverlayButton;
        public Transform levelContainer;
        public Preloader preloader;
        public GameObject levelSelector;
        public GameObject loadingScreen;
        public GameObject createOverlay;
        public InputField levelNameInput;

        private Level _selectedLevel;
        private readonly List<LevelSelector> _levelSelectors = new List<LevelSelector>();

        private void Start()
        {
            backButton.onClick.AddListener(OnBackClick);
            playButton.onClick.AddListener(OnPlayClick);
            playButton.onClick.AddListener(OnPlayClick);
            deleteButton.onClick.AddListener(OnDeleteClick);
            createButton.onClick.AddListener(OnCreateClick);
            showCreateOverlayButton.onClick.AddListener(OnCreateOverlayClick);
            hideCreateOverlayButton.onClick.AddListener(OnHideOverlayClick);

            loadingScreen.SetActive(false);
            createOverlay.SetActive(false);
            CreateLevelSelectors();
        }

        private void OnGUI()
        {
            playButton.interactable = _selectedLevel != null;
            deleteButton.interactable = _selectedLevel != null;
        }

        private void OnHideOverlayClick()
        {
            createOverlay.SetActive(false);
        }

        private void OnCreateOverlayClick()
        {
            createOverlay.SetActive(true);
        }

        private void OnCreateClick()
        {
            preloader.max = WorldGenerator.StarSystemsCount;
            createOverlay.SetActive(false);
            loadingScreen.SetActive(true);
            
            string inputValue = levelNameInput.text;
            string levelName = inputValue.Length > 0 ? inputValue : "new universe";
            Level level = Game.App.LevelManager.CreateLevel(WorldGenerator.WorldSeed, levelName);
            Game.App.LevelManager.Store();
            
            WorldGenerator.Generate().Then(generated => { SceneManager.LoadScene(GameSystem); });
        }

        private void OnLevelSelected(LevelSelector selector)
        {
            _levelSelectors.ForEach(levelSelectorComponent => { levelSelectorComponent.SetSelected(false); });

            selector.SetSelected(true);
            _selectedLevel = selector.Level;
        }

        private void OnDeleteClick()
        {
            Game.App.LevelManager.DeleteLevel(_selectedLevel);
            Game.App.LevelManager.Store();

            LevelSelector levelSelectorToDelete = GetLevelSelectorByLevelId(_selectedLevel.Id);
            _levelSelectors.Remove(levelSelectorToDelete);

            _selectedLevel = null;
            Destroy(levelSelectorToDelete.gameObject);
        }

        private static void OnPlayClick()
        {
            SceneManager.LoadScene(GameSystem);
        }

        private static void OnBackClick()
        {
            SceneManager.LoadScene(MainMenu);
        }

        private LevelSelector GetLevelSelectorByLevelId(int id)
        {
            return _levelSelectors.Find(levelSelectorComponent => levelSelectorComponent.Level.Id == id);
        }

        private void CreateLevelSelectors()
        {
            foreach (Level level in Game.App.LevelManager.Levels)
            {
                GameObject levelObject = Instantiate(levelSelector, levelContainer);
                LevelSelector levelSelectorComponent = levelObject.GetComponent<LevelSelector>();
                levelSelectorComponent.MouseClickEvent.AddListener(() => OnLevelSelected(levelSelectorComponent));
                levelSelectorComponent.MouseDoubleClickEvent.AddListener(() =>
                {
                    OnLevelSelected(levelSelectorComponent);
                    OnPlayClick();
                });
                levelSelectorComponent.Level = level;

                _levelSelectors.Add(levelSelectorComponent);
            }
        }
    }
}