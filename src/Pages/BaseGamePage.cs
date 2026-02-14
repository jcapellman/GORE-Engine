using System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using GORE.Models;
using GORE.GameEngine;
using GORE.Services;

namespace GORE.Pages
{
    /// <summary>
    /// Base game page providing complete RPG framework.
    /// Games inherit this and implement sprite rendering methods.
    /// </summary>
    public abstract class BaseGamePage : BasePage
    {
        // Core game state
        protected GameState gameState;
        protected BattleSystem battleSystem;
        protected InputManager inputManager;
        protected Map currentMap;
        protected Character hero;
        protected DispatcherTimer gameTimer;
        
        // UI state
        protected bool isMenuOpen = false;
        protected int battleMenuSelection = 0;
        protected const int BattleMenuItemCount = 4;
        protected int inGameMenuSelection = 0;
        protected const int InGameMenuItemCount = 3;
        protected bool isDialogOpen = false;
        
        // Canvas resources
        protected Microsoft.Graphics.Canvas.CanvasBitmap battleBackgroundBitmap;
        
        // Animation state
        protected float animationTime = 0;

        protected BaseGamePage()
        {
            InitializeGameTimer();
        }

        private void InitializeGameTimer()
        {
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60fps
            };
            gameTimer.Tick += OnGameTimerTick;
            gameTimer.Start();
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Window.Current.CoreWindow.KeyDown += OnCoreWindowKeyDown;

            if (e.Parameter is string heroName)
            {
                InitializeNewGame(heroName);
            }
            else if (e.Parameter is SaveData saveData)
            {
                LoadGame(saveData);
            }
            else
            {
                InitializeNewGame("Hero");
            }
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Window.Current.CoreWindow.KeyDown -= OnCoreWindowKeyDown;
            gameTimer.Stop();
        }

        // Game lifecycle methods
        protected virtual void InitializeNewGame(string heroName)
        {
            hero = new Character(heroName)
            {
                Level = 1,
                MaxHP = 100,
                CurrentHP = 100,
                MaxMP = 50,
                CurrentMP = 50,
                Attack = 20,
                Defense = 15,
                Magic = 18,
                Speed = 12,
                X = 5,
                Y = 5
            };

            currentMap = new Map(20, 15);
            currentMap.GenerateMap();

            gameState = GameState.Exploration;
            battleSystem = new BattleSystem();
            inputManager = new InputManager();

            MusicManager.PlayMusic(MusicTrack.Exploration);
            UpdateUI();
        }

        protected virtual void LoadGame(SaveData saveData)
        {
            hero = saveData.ToCharacter();

            currentMap = new Map(20, 15);
            currentMap.GenerateMap();

            gameState = GameState.Exploration;
            battleSystem = new BattleSystem();
            inputManager = new InputManager();

            MusicManager.PlayMusic(MusicTrack.Exploration);
            UpdateUI();
        }

        // Abstract rendering methods (game-specific implementation)
        protected abstract void DrawExplorationMode(CanvasDrawingSession session);
        protected abstract void DrawBattleMode(CanvasDrawingSession session);
        protected abstract void DrawHeroSprite(CanvasDrawingSession session, float x, float y);
        protected abstract void DrawEnemySprite(CanvasDrawingSession session, float x, float y, string enemyName);

        // Game timer tick
        protected virtual void OnGameTimerTick(object sender, object e)
        {
            animationTime += 0.016f;
            // Derived classes can override for custom animation
        }

        // Input handling
        protected virtual void OnCoreWindowKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            args.Handled = true;

            if (isDialogOpen) return;

            if (args.VirtualKey == Windows.System.VirtualKey.Escape)
            {
                ToggleMenu();
                return;
            }

            if (isMenuOpen)
            {
                HandleInGameMenuInput(args.VirtualKey);
                return;
            }

            if (gameState == GameState.Battle)
            {
                HandleBattleInput(args.VirtualKey);
            }
            else if (gameState == GameState.Exploration)
            {
                HandleExplorationInput(args.VirtualKey);
            }
        }

        // Menu handling
        protected virtual void HandleInGameMenuInput(Windows.System.VirtualKey key)
        {
            if (key == Windows.System.VirtualKey.Up || key == Windows.System.VirtualKey.W)
            {
                inGameMenuSelection--;
                if (inGameMenuSelection < 0)
                    inGameMenuSelection = InGameMenuItemCount - 1;
                OnInGameMenuCursorChanged();
            }
            else if (key == Windows.System.VirtualKey.Down || key == Windows.System.VirtualKey.S)
            {
                inGameMenuSelection++;
                if (inGameMenuSelection >= InGameMenuItemCount)
                    inGameMenuSelection = 0;
                OnInGameMenuCursorChanged();
            }
            else if (key == Windows.System.VirtualKey.Enter || key == Windows.System.VirtualKey.Space)
            {
                ExecuteInGameMenuSelection();
            }
        }

        protected virtual void HandleBattleInput(Windows.System.VirtualKey key)
        {
            if (battleSystem.CurrentEnemy == null) return;

            if (key == Windows.System.VirtualKey.Up || key == Windows.System.VirtualKey.W)
            {
                battleMenuSelection--;
                if (battleMenuSelection < 0)
                    battleMenuSelection = BattleMenuItemCount - 1;
                OnBattleMenuCursorChanged();
            }
            else if (key == Windows.System.VirtualKey.Down || key == Windows.System.VirtualKey.S)
            {
                battleMenuSelection++;
                if (battleMenuSelection >= BattleMenuItemCount)
                    battleMenuSelection = 0;
                OnBattleMenuCursorChanged();
            }
            else if (key == Windows.System.VirtualKey.Enter || key == Windows.System.VirtualKey.Space)
            {
                ExecuteBattleCommand(battleMenuSelection);
            }
        }

        protected virtual void HandleExplorationInput(Windows.System.VirtualKey key)
        {
            int newX = hero.X;
            int newY = hero.Y;

            switch (key)
            {
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.W:
                    newY--;
                    break;
                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.S:
                    newY++;
                    break;
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.A:
                    newX--;
                    break;
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.D:
                    newX++;
                    break;
            }

            if (currentMap.IsWalkable(newX, newY))
            {
                hero.X = newX;
                hero.Y = newY;

                Random random = new();
                if (random.Next(100) < 10) // 10% encounter rate
                {
                    StartBattle();
                }
            }

            UpdateUI();
        }

        // Battle system
        protected virtual void StartBattle()
        {
            gameState = GameState.Battle;
            battleSystem.StartBattle(hero);
            battleMenuSelection = 0;
            OnBattleStarted();
            MusicManager.PlayMusic(MusicTrack.Battle);
            UpdateUI();
        }

        protected virtual void ExecuteBattleCommand(int commandIndex)
        {
            switch (commandIndex)
            {
                case 0: ExecuteAttackCommand(); break;
                case 1: ExecuteMagicCommand(); break;
                case 2: ExecuteItemCommand(); break;
                case 3: ExecuteDefendCommand(); break;
            }
        }

        protected virtual void ExecuteAttackCommand()
        {
            if (battleSystem.CurrentEnemy == null) return;

            string result = battleSystem.ExecutePlayerAttack();
            OnBattleMessage(result);

            if (battleSystem.CurrentEnemy.CurrentHP <= 0)
            {
                EndBattle(true);
            }
            else
            {
                string enemyAction = battleSystem.ExecuteEnemyTurn();
                OnBattleMessage(result + "\n" + enemyAction);

                if (hero.CurrentHP <= 0)
                    EndBattle(false);
            }

            UpdateUI();
        }

        protected virtual void ExecuteMagicCommand()
        {
            if (hero.CurrentMP >= 10)
            {
                hero.CurrentMP -= 10;
                int damage = (int)(hero.Magic * 1.5);
                battleSystem.CurrentEnemy.CurrentHP -= damage;
                OnBattleMessage($"{hero.Name} casts Fire! Deals {damage} damage!");

                if (battleSystem.CurrentEnemy.CurrentHP <= 0)
                {
                    EndBattle(true);
                }
                else
                {
                    string enemyAction = battleSystem.ExecuteEnemyTurn();
                    OnBattleMessage($"{hero.Name} casts Fire! Deals {damage} damage!\n{enemyAction}");

                    if (hero.CurrentHP <= 0)
                        EndBattle(false);
                }
            }
            else
            {
                OnBattleMessage("Not enough MP!");
            }

            UpdateUI();
        }

        protected virtual void ExecuteItemCommand()
        {
            hero.CurrentHP = Math.Min(hero.CurrentHP + 30, hero.MaxHP);
            string enemyAction = battleSystem.ExecuteEnemyTurn();
            OnBattleMessage($"{hero.Name} uses Potion! Restored 30 HP!\n{enemyAction}");

            if (hero.CurrentHP <= 0)
                EndBattle(false);

            UpdateUI();
        }

        protected virtual void ExecuteDefendCommand()
        {
            int originalDefense = hero.Defense;
            hero.Defense = (int)(hero.Defense * 1.5);

            string enemyAction = battleSystem.ExecuteEnemyTurn();
            OnBattleMessage($"{hero.Name} defends!\n{enemyAction}");

            hero.Defense = originalDefense;

            if (hero.CurrentHP <= 0)
                EndBattle(false);

            UpdateUI();
        }

        protected virtual void EndBattle(bool victory)
        {
            gameState = GameState.Exploration;
            OnBattleEnded();

            if (victory)
            {
                int expGained = 50;
                OnBattleMessage($"Victory! Gained {expGained} EXP!");
                hero.CurrentHP = Math.Min(hero.CurrentHP + 20, hero.MaxHP);
                hero.CurrentMP = Math.Min(hero.CurrentMP + 10, hero.MaxMP);
                MusicManager.PlayMusic(MusicTrack.Victory);

                var musicTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                musicTimer.Tick += (s, e) =>
                {
                    MusicManager.PlayMusic(MusicTrack.Exploration);
                    ((DispatcherTimer)s).Stop();
                };
                musicTimer.Start();
            }
            else
            {
                OnBattleMessage("Defeated! Game Over!");
                hero.CurrentHP = hero.MaxHP;
                hero.CurrentMP = hero.MaxMP;
                MusicManager.PlayMusic(MusicTrack.GameOver);
            }

            UpdateUI();
        }

        protected virtual void ToggleMenu()
        {
            if (gameState == GameState.Battle) return;

            isMenuOpen = !isMenuOpen;
            OnMenuToggled(isMenuOpen);

            if (isMenuOpen)
            {
                inGameMenuSelection = 0;
                OnInGameMenuCursorChanged();
            }
        }

        protected virtual void ExecuteInGameMenuSelection()
        {
            switch (inGameMenuSelection)
            {
                case 0: OnResumeGame(); break;
                case 1: if (gameState == GameState.Exploration) OnSaveGame(); break;
                case 2: OnReturnToMainMenu(); break;
            }
        }

        // Abstract UI update hooks (implemented by derived class)
        protected abstract void UpdateUI();
        protected abstract void OnBattleMenuCursorChanged();
        protected abstract void OnInGameMenuCursorChanged();
        protected abstract void OnBattleStarted();
        protected abstract void OnBattleEnded();
        protected abstract void OnBattleMessage(string message);
        protected abstract void OnMenuToggled(bool isOpen);
        protected abstract void OnResumeGame();
        protected abstract void OnSaveGame();
        protected abstract void OnReturnToMainMenu();
    }
}
