﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;
using BlisterUI.Widgets;
using System.Diagnostics;

namespace RTS {
    public class MenuScreen : GameScreen<App> {
        public override int Next {
            get;
            protected set;
        }
        public override int Previous {
            get { return -1; }
            protected set { }
        }

        const int BUTTON_SPACING_X = 10;
        const int BUTTON_SPACING_Y = 10;
        const int MENU_TEXT_SIZE_Y = 80;

        WidgetRenderer wr;
        IDisposable fontDisp;
        RectButton[] buttons;
        TextWidget[] buttonsText;
        TextWidget txtMainMenu;
        SoundEffect seHover, seClick;
        Texture2D[] tPanels;

        public override void Build() {
            using(var s = File.OpenRead(@"Content\Audio\btnClick.wav")) {
                seClick = SoundEffect.FromStream(s);
            }
        }
        public override void Destroy(GameTime gameTime) {
            seClick.Dispose();
        }

        public override void OnEntry(GameTime gameTime) {
            SpriteFont f = XNASpriteFont.Compile(G, "Arial", 32, out fontDisp);
            wr = new WidgetRenderer(G, f);

            using(var s = File.OpenRead(@"Content\Audio\btnHover.wav")) {
                seHover = SoundEffect.FromStream(s);
            }

            buttons = new RectButton[4];
            buttonsText = new TextWidget[buttons.Length];
            tPanels = new Texture2D[buttons.Length];
            int w = G.Viewport.Width;
            w -= (buttons.Length + 1) * BUTTON_SPACING_X;
            w /= buttons.Length;
            ButtonHighlightOptions o1 = new ButtonHighlightOptions(w, G.Viewport.Height - BUTTON_SPACING_Y * 3 - MENU_TEXT_SIZE_Y, Color.DarkGray);
            ButtonHighlightOptions o2 = new ButtonHighlightOptions(o1.Width, o1.Height, Color.RoyalBlue);
            for(int i = 0; i < buttons.Length; i++) {
                using(var s = File.OpenRead(@"Content\UI\MainP" + (i + 1) + ".png")) {
                    tPanels[i] = Texture2D.FromStream(G, s);
                }
                buttons[i] = new RectButton(wr, o1, o2, tPanels[i]);
                buttons[i].Hook();
                buttons[i].OnButtonPress += MenuScreen_OnButtonPress;
                buttons[i].OnMouseEntry += MenuScreen_OnMouseEntry;
                buttons[i].LayerDepth = 1f;
                buttons[i].OffsetAlignX = Alignment.RIGHT;
                buttons[i].Offset = new Point(BUTTON_SPACING_X, 0);
                if(i > 0)
                    buttons[i].Parent = buttons[i - 1];

                buttonsText[i] = new TextWidget(wr);
                buttonsText[i].Font = f;
                buttonsText[i].OffsetAlignX = Alignment.MID;
                buttonsText[i].OffsetAlignY = Alignment.TOP;
                buttonsText[i].Offset = new Point(0, 30);
                buttonsText[i].AlignX = Alignment.MID;
                buttonsText[i].AlignY = Alignment.MID;
                buttonsText[i].Parent = buttons[i];
                buttonsText[i].LayerDepth = 0.9f;
                buttonsText[i].Color = Color.Black;
                buttonsText[i].Height = 20;
            }
            buttonsText[0].Text = "Play Game";
            buttonsText[1].Text = "Army Painter";
            buttonsText[2].Text = "Options";
            buttonsText[3].Text = "Exit";
            buttons[0].Anchor = new Point(BUTTON_SPACING_X, MENU_TEXT_SIZE_Y + 2 * BUTTON_SPACING_Y);

            txtMainMenu = new TextWidget(wr);
            txtMainMenu.Anchor = new Point(G.Viewport.Width / 2, BUTTON_SPACING_Y);
            txtMainMenu.Height = MENU_TEXT_SIZE_Y;
            txtMainMenu.AlignX = Alignment.MID;
            txtMainMenu.Color = Color.White;
            txtMainMenu.Text = "Main Menu";

            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;

            if(fontDisp != null) {
                fontDisp.Dispose();
                fontDisp = null;
            }

            foreach(var button in buttons) {
                button.OnButtonPress -= MenuScreen_OnButtonPress;
                button.OnMouseEntry -= MenuScreen_OnMouseEntry;
                button.Dispose();
            }
            buttons = null;
            foreach(var bt in buttonsText) {
                bt.Dispose();
            }
            buttonsText = null;
            foreach(var t in tPanels) {
                t.Dispose();
            }
            tPanels = null;
            wr.Dispose();

            seHover.Dispose();
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            wr.Draw(SB);
        }

        private void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Escape:
                    State = ScreenState.ExitApplication;
                    break;
            }
        }
        private void MenuScreen_OnButtonPress(RectButton obj) {
            seClick.Play();
            if(obj == buttons[0]) {
                Next = game.LoadScreen.Index;
                State = ScreenState.ChangeNext;
            }
            else if(obj == buttons[1]) {
                Next = game.ColorSchemeScreen.Index;
                State = ScreenState.ChangeNext;
            }
            else if(obj == buttons[2]) {
                RTSEngine.Data.UserConfig.UseFullscreen = !RTSEngine.Data.UserConfig.UseFullscreen;
                RTSEngine.Data.UserConfig.Save(App.USER_CONFIG_FILE_PATH);
                ProcessStartInfo psi = new ProcessStartInfo("RTS.exe");
                psi.WorkingDirectory = Process.GetCurrentProcess().StartInfo.WorkingDirectory;
                Process.Start(psi);
                State = ScreenState.ExitApplication;
            }
            else if(obj == buttons[3]) {
                State = ScreenState.ExitApplication;
            }
        }
        private void MenuScreen_OnMouseEntry(RectButton obj) {
            seHover.Play();
        }
    }
}