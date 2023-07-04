using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;


/*
 * TextInput Class
 * -fw- from http://www.dev42.de
 * 
 * Gamecomponent to map Textinput to KeyEvents.
 * 
 */

namespace Water3D {
    /// <summary>
    /// TextInput Event Arguments
    /// </summary>
    class TextInputEventArgs : EventArgs {
        private Keys _key;
        private String _translated;
        /// <summary>
        /// The signalled Key
        /// </summary>
        public Keys Key 
        {
            get 
            {
                return _key;
            }
        }
        /// <summary>
        /// The Translated Key
        /// </summary>
        public String Translated 
        {
            get 
            {
                return _translated;
            }
        }
        
        public TextInputEventArgs( Keys key , String translated) 
        {
            _key = key;
            _translated = translated;
        }
    }
    /// <summary>
    /// Tranlator Event Arguments
    /// </summary>
    class TranslatorEventArgs : EventArgs 
    {
        private Keys _key;
        private String _translated;
        private Boolean _isTranslated = false;
        /// <summary>
        /// Indicates if this Argument allready has been translated 
        /// by another Translator
        /// </summary>
        public Boolean IsTranslated 
        {
            get 
            {
                return _isTranslated;
            }
        }
        /// <summary>
        /// The Original Key
        /// </summary>
        public Keys Key 
        {
            get 
            {
                return _key;
            }
        }
        /// <summary>
        /// The Changable String it will be translated to.
        /// This string will be passed to the Key...Events
        /// </summary>
        public String Translated 
        {
            get 
            {
                return _translated;
            }
            set
            { 
                _translated = value != null ? value : "";
                _isTranslated = true;
            }
        }

        public TranslatorEventArgs( Keys key, String translated ) 
        {
            _key = key;
            _translated = translated;
        }
    }
    /// <summary>
    /// TextInput GameComponent
    /// </summary>
    class TextInput : GameComponent, IUpdateable
    {
        #region Members
        /// <summary>
        /// Regular Expression defining all allowed Keys
        /// </summary>
        private Regex _inputExpression;
        /// <summary>
        /// Buffer for Keystate of previous Call
        /// </summary>
        private KeyboardState _previousKeyState;
        /// <summary>
        /// Enable Disable Flag
        /// </summary>
        private Boolean _isEnabled;
        
        #endregion

        #region Thrown Events

        /// <summary>
        /// OnKeyDown raised before OnTranslating and OnKeyPress, but translated String is not used
        /// </summary>
        public event EventHandler<TextInputEventArgs> OnKeyDown;
        /// <summary>
        /// OnKeyUp raised if key is going up, translated string is not used
        /// </summary>
        public event EventHandler<TextInputEventArgs> OnKeyUp;
        /// <summary>
        /// OnKeyPress is raised after OnTranslating, the parameters were the original key and the tranlated string
        /// </summary>
        public event EventHandler<TextInputEventArgs> OnKeyPress;
        /// <summary>
        /// OnTranslatig is raised right before OnKeyPress. In translated String a new Value kann be stored.
        /// The translated Value is passed to the OnKeyPress 
        /// </summary>
        public event EventHandler<TranslatorEventArgs> OnTranslating;

        #endregion

        public TextInput(Game game) : base(game)
        {
            _isEnabled = true;
            _inputExpression = new Regex("^([a-zA-Z]|Space|D[0-9]|OemComma|OemPeriod|OemPlus|OemMinus|Multiply|Divide){1}$");
        }

        /// <summary>
        /// Retrieves the Keys  of the current Keystate
        /// </summary>
        /// <param name="keystate"></param>
        private void processKeystate(KeyboardState keystate) 
        {
            // Extract all Keypressed / KeyDown eventes from current State
            if(keystate.GetPressedKeys().Length != 0) 
            {
                Keys[] pressedKeys = keystate.GetPressedKeys();
                foreach(Keys currentPressedKey in pressedKeys) 
                {
                    if(keystate.IsKeyDown(currentPressedKey)) 
                    {
                        if(_previousKeyState.IsKeyUp(currentPressedKey)) 
                        {
                            raiseKeyDown(currentPressedKey, keystate);
                        }
                    }
                }
            }

            // Extract all KeyUp Events
            if(_previousKeyState.GetPressedKeys().Length != 0) 
            {
                Keys[] previouspressedKeys = _previousKeyState.GetPressedKeys();
                foreach(Keys currentReleasedKey in previouspressedKeys) 
                {
                    if(keystate.IsKeyUp(currentReleasedKey)) 
                    {
                        if(_previousKeyState.IsKeyDown(currentReleasedKey)) 
                        {
                            raiseKeyUp(currentReleasedKey, keystate);
                        }
                    }
                }
            }
            _previousKeyState = keystate;
        }
        
        /// <summary>
        /// Raises KeyDown Translated and KeyPress Events
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keystate"></param>
        private void raiseKeyDown( Keys key, KeyboardState keystate )
        {
            // First call own Translation
            String translated = translateKey(key, keystate);

            // Call Registered Translators
            if(OnTranslating != null) 
            {
                TranslatorEventArgs args = new TranslatorEventArgs(key, translated);
                OnTranslating(this, args);

                if(args.IsTranslated)
                    translated = args.Translated;
            }

            // Raise KeyDown 
            if(OnKeyDown != null)
                OnKeyDown(this, new TextInputEventArgs(key, translated));

            // Raise KeyPress
            if(OnKeyPress != null)
                OnKeyPress(this, new TextInputEventArgs(key, translated));
        }
        /// <summary>
        /// Raises Translation and KeyUp Event
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keystate"></param>
        private void raiseKeyUp( Keys key, KeyboardState keystate )
        {
            // First call own Translation
            String translated = translateKey(key, keystate);
            if(OnTranslating != null) {
                TranslatorEventArgs args = new TranslatorEventArgs(key, translated);
                OnTranslating(this, args);
                if(args.IsTranslated)
                    translated = args.Translated;
            }

            // Raise OnKeyUp
            if(OnKeyUp != null) {
                OnKeyUp(this, new TextInputEventArgs(key, translated));
            }
        }
        /// <summary>
        /// translates Keys with fix translationMapping before Translation Event is called
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keystate"></param>
        /// <returns></returns>
        private String translateKey(Keys key, KeyboardState keystate) 
        {
            String translated = "";
            string pressed = key.ToString();

            if(_inputExpression.IsMatch(pressed)) {
                // Check for Big Letters
                if(!(keystate.IsKeyDown(Keys.RightShift) || keystate.IsKeyDown(Keys.LeftShift))) 
                {
                    translated = pressed.ToLower();
                } 
                else
                {
                    translated = pressed.ToUpper();
                }
                // Check for Digital Numbers
                if(pressed.Length == 2 && pressed.ToLower().StartsWith("d")) 
                {
                    translated = pressed.Substring(1, 1);
                    // test special characters above numbers
                    if (keystate.IsKeyDown(Keys.RightShift) || keystate.IsKeyDown(Keys.LeftShift))
                    {
                        switch (translated)
                        {
                            case("1"):
                                translated = "!";
                                break;
                            case ("2"):
                                translated = "\"";
                                break;
                            case ("3"):
                                translated = "§";
                                break;
                            case ("4"):
                                translated = "$";
                                break;
                            case ("5"):
                                translated = "%";
                                break;
                            case ("6"):
                                translated = "&";
                                break;
                            case ("7"):
                                translated = "/";
                                break;
                            case ("8"):
                                translated = "(";
                                break;
                            case ("9"):
                                translated = ")";
                                break;
                            case ("0"):
                                translated = "=";
                                break;
                        }
                    }
                    else if (keystate.IsKeyDown(Keys.RightAlt))
                    {
                        switch (translated)
                        {
                            case ("2"):
                                translated = "²";
                                break;
                            case ("3"):
                                translated = "³";
                                break;
                             case ("7"):
                                translated = "{";
                                break;
                            case ("8"):
                                translated = "[";
                                break;
                            case ("9"):
                                translated = "]";
                                break;
                            case ("0"):
                                translated = "}";
                                break;
                        }
                    }
                }
                // Check for Space
                switch(pressed.ToLower()) 
                {
                    case "space":
                        translated = " ";
                        break;
                }
                // special keys
                switch (pressed.ToLower())
                {
                    case "oemcomma":
                        translated = ",";
                        break;
                    case "oemperiod":
                        translated = ".";
                        break;
                    case "oemplus":
                        translated = "+";
                        break;
                    case "oemminus":
                        translated = "-";
                        break;
                }
            }
            return translated;
        }
        #region IUpdateable Members
        /// <summary>
        /// Enables or disables the Component
        /// In disabled mode no events were thrown
        /// </summary>
        new public bool Enabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
            }
        }

        /// <summary>
        /// Update called on every registered Gamecomponent
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update( GameTime gameTime )
        {
            if(Enabled) 
            {
                KeyboardState key = Keyboard.GetState();
                processKeystate(key);
            }
        }

        #endregion
    }
}
