using System.Collections.Generic;
using System.Numerics;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Scripting
{
    [MoonSharpUserData]
    public class LuaPluginState
    {
        /// <summary>
        ///     The time elapsed between the previous and current frame
        /// </summary>
        public double DeltaTime { get; set; }

        /// <summary>
        ///     Unix timestmap of the current time
        /// </summary>
        public long UnixTime { get; set; }

        /// <summary>
        ///     If the plugin window is currently hovered.
        ///     This has to be set by the plugin itself
        /// </summary>
        public bool IsWindowHovered { get; set; }

        /// <summary>
        ///     Any state that the user wants to store for their plugin
        /// </summary>
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        /// <summary>
        ///     Width and height of the current Quaver window
        /// </summary>
        public Vector2 WindowSize { get; set; }

        /// <summary>
        ///     Gets a value at a particular key
        /// </summary>
        /// <param name="key"></param>
        public object GetValue(string key) => Values.GetValueOrDefault(key);

        /// <summary>
        ///     Sets a value at a particular key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, object value) => Values[key] = value;
    }
}
