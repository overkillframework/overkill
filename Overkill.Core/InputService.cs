using Microsoft.Extensions.Logging;
using Overkill.Common.Enums;
using Overkill.Common.Exceptions;
using Overkill.Core.Interfaces;
using Overkill.Core.Topics;
using Overkill.Core.Topics.Input;
using Overkill.PubSub.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Overkill.Core
{
    /// <summary>
    /// Manages registered inputs for manual driving by users through a front-end application.
    /// </summary>
    public class InputService : IInputService
    {
        private readonly ILogger<InputService> _logger;
        private readonly IOverkillConfiguration _config;

        private readonly Dictionary<string, KeyboardKey> _defaultKeyboardBindings;
        private readonly Dictionary<string, GamepadInput> _defaultGamepadBindings;

        private readonly Dictionary<string, Action<InputState>> _keyboardBindings;
        private readonly Dictionary<string, Action<(bool isPressed, float x, float y)>> _gamepadJoystickBindings;
        private readonly Dictionary<string, Action<InputState>> _gamepadButtonBindings;
        private readonly Dictionary<string, Action<float>> _gamepadTriggerBindings;

        public InputService(
            ILogger<InputService> logger,
            IPubSubService pubSub, 
            IOverkillConfiguration config
        )
        {
            _logger = logger;
            _config = config;

            _defaultKeyboardBindings = new Dictionary<string, KeyboardKey>();
            _defaultGamepadBindings = new Dictionary<string, GamepadInput>();

            _keyboardBindings = new Dictionary<string, Action<InputState>>();
            _gamepadJoystickBindings = new Dictionary<string, Action<(bool isPressed, float x, float y)>>();
            _gamepadButtonBindings = new Dictionary<string, Action<InputState>>();
            _gamepadTriggerBindings = new Dictionary<string, Action<float>>();

            pubSub.Subscribe<KeyboardInputTopic>(topic =>
            {
                EmitKeyboardEvent(topic.Name, topic.IsPressed ? InputState.Pressed : InputState.Released);
            });

            pubSub.Subscribe<GamepadJoystickInputTopic>(topic =>
            {
                EmitGamepadJoystickEvent(topic.Name, topic.IsPressed, topic.X, topic.Y);
            });

            pubSub.Subscribe<GamepadButtonInputTopic>(topic =>
            {
                EmitGamepadButtonEvent(topic.Name, topic.State);
            });

            pubSub.Subscribe<GamepadTriggerInputTopic>(topic =>
            {
                EmitGamepadTriggerEvent(topic.Name, topic.Value);
            });
        }

        /// <summary>
        /// Binds a keyboard key to an action, by a reference name.
        /// Throws an exception if an action already exists for the input, it is not defined in the configuration, or the value is invalid.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultBinding"></param>
        /// <param name="action"></param>
        public void Keyboard(string name, KeyboardKey defaultBinding, Action<InputState> action)
        {
            _logger.LogInformation("Binding keyboard input: {name}, key: {key}", name, defaultBinding);

            if (!_config.Input.Keyboard.ContainsKey(name) || !Enum.IsDefined(typeof(KeyboardKey), _config.Input.Keyboard[name]))
            {
                _logger.LogWarning("Failed to bind input. Not specified in configuration file.");
                throw new InvalidInputConfigurationException(name);
            }
            
            if (_keyboardBindings.ContainsKey(name))
            {
                _logger.LogWarning("Failed to bind input. Already bound elsewhere.");
                throw new InputAlreadyBoundException(name);
            }
            
            _defaultKeyboardBindings.Add(name, defaultBinding);
            _keyboardBindings.Add(name, action);
        }

        /// <summary>
        /// Binds a joystick event to an action, by a reference name.
        /// Throws an exception if an action already exists for the input, it is not defined in the configuration, or the value is invalid.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void GamepadJoystick(string name, GamepadInput defaultBinding, Action<(bool isPressed, float x, float y)> action)
        {
            _logger.LogInformation("Binding joystick input: {name}, joystick: {joystick}", name, defaultBinding);

            if (!_config.Input.Gamepad.ContainsKey(name) || !Enum.IsDefined(typeof(GamepadInput), _config.Input.Gamepad[name]))
            {
                _logger.LogWarning("Failed to bind input. Not specified in configuration file.");
                throw new InvalidInputConfigurationException(name);
            }
            
            if (_gamepadJoystickBindings.ContainsKey(name))
            {
                _logger.LogWarning("Failed to bind input. Already bound elsewhere.");
                throw new InputAlreadyBoundException(name);
            }
            
            _defaultGamepadBindings.Add(name, defaultBinding);
            _gamepadJoystickBindings.Add(name, action);
        }

        /// <summary>
        /// Binds a gamepad trigger to an action, by a reference name.
        /// Throws an exception if an action already exists for the input, it is not defined in the configuration, or the value is invalid.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultBinding"></param>
        /// <param name="action"></param>
        public void GamepadTrigger(string name, GamepadInput defaultBinding, Action<float> action)
        {
            _logger.LogInformation("Binding gamepad trigger input: {name}, trigger: {trigger}", name, defaultBinding);

            if (!_config.Input.Gamepad.ContainsKey(name) || !Enum.IsDefined(typeof(GamepadInput), _config.Input.Gamepad[name]))
            {
                _logger.LogWarning("Failed to bind input. Not specified in configuration file.");
                throw new InvalidInputConfigurationException(name);
            }
            
            if (_gamepadTriggerBindings.ContainsKey(name))
            {
                _logger.LogWarning("Failed to bind input. Already bound elsewhere.");
                throw new InputAlreadyBoundException(name);
            }
            
            _defaultGamepadBindings.Add(name, defaultBinding);
            _gamepadTriggerBindings.Add(name, action);
        }

        /// <summary>
        /// Binds a gamepad button to an action, by a reference name.
        /// Throws an exception if an action already exists for the input, it is not defined in the configuration, or the value is invalid.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void GamepadButton(string name, GamepadInput defaultBinding, Action<InputState> action)
        {
            _logger.LogInformation("Binding gamepad button input: {name}, button: {button}", name, defaultBinding);

            if (!_config.Input.Gamepad.ContainsKey(name) || !Enum.IsDefined(typeof(GamepadInput), _config.Input.Gamepad[name]))
            {
                _logger.LogWarning("Failed to bind input. Not specified in configuration file.");
                throw new InvalidInputConfigurationException(name);
            }
            
            if (_gamepadButtonBindings.ContainsKey(name))
            {
                _logger.LogWarning("Failed to bind input. Already bound elsewhere.");
                throw new InputAlreadyBoundException(name);
            }
            
            _defaultGamepadBindings.Add(name, defaultBinding);
            _gamepadButtonBindings.Add(name, action);
        }

        /// <summary>
        /// Returns a dictionary of (input name, default key code)
        /// </summary>
        public Dictionary<string, int> GetKeyboardInputs()
        {
            return _keyboardBindings
                .ToDictionary(
                    x => x.Key, 
                    x => (int)_defaultKeyboardBindings[x.Key]
                );
        }

        /// <summary>
        /// Returns a dictionary of gamepad inputs (input name, default button index)
        /// </summary>
        public Dictionary<string, int> GetGamepadInputs()
        {
            return _gamepadJoystickBindings
                .ToDictionary(
                    x => x.Key, 
                    x => (int)_defaultGamepadBindings[x.Key]
                );
        }

        /// <summary>
        /// Called internally via PubSub subscription, used to invoke a callback action
        /// </summary>
        /// <param name="name">The friendly input name</param>
        /// <param name="state">The state of the keyboard key, pressed or released</param>
        private void EmitKeyboardEvent(string name, InputState state)
        {
            if (!_keyboardBindings.ContainsKey(name))
            {
                _logger.LogDebug("Keyboard event emitted for {name} {state}, but no bindings found", name, state);
                return;
            }
            
            _keyboardBindings[name](state);
        }

        /// <summary>
        /// Called internally via PubSub subscription, used to invoke a callback function
        /// </summary>
        /// <param name="name">The friendly input name</param>
        /// <param name="x">The X axis of the joystick</param>
        /// <param name="y">The Y axis of the joystick</param>
        private void EmitGamepadJoystickEvent(string name, bool isPressed, float x, float y)
        {
            if (!_gamepadJoystickBindings.ContainsKey(name))
            {
                _logger.LogDebug("Joystick event emitted for {name}, but no bindings found", name);
                return;
            }
            _gamepadJoystickBindings[name]((isPressed, x, y));
        }

        /// <summary>
        /// Called internally via PubSub subscription, used to invoke a callback function
        /// </summary>
        /// <param name="name">The friendly input name</param>
        /// <param name="state">The state of the gamepad button, pressed or released</param>
        private void EmitGamepadButtonEvent(string name, InputState state)
        {
            if (!_gamepadButtonBindings.ContainsKey(name))
            {
                _logger.LogDebug("Gamepad button event emitted for {name} {state}, but no bindings found", name, state);
                return;
            }
            _gamepadButtonBindings[name](state);
        }

        /// <summary>
        /// Called internally via PubSub subscription, used to invoke a callback function
        /// </summary>
        /// <param name="name">The friendly input name</param>
        /// <param name="value">The value of the trigger (how held down it is)</param>
        private void EmitGamepadTriggerEvent(string name, float value)
        {
            if (!_gamepadTriggerBindings.ContainsKey(name))
            {
                _logger.LogDebug("Gamepad trigger event emitted for {name}, but no bindings found", name);
                return;
            }
            _gamepadTriggerBindings[name](value);
        }
    }
}
