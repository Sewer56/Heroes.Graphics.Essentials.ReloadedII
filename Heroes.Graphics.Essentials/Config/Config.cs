using System.ComponentModel;
using Heroes.Graphics.Essentials.Configuration;
using Heroes.Graphics.Essentials.Heroes;

namespace Heroes.Graphics.Essentials.Config
{
    public class Config : Configurable<Config>
    {
        [DisplayName("Width")]
        [Description("Width of the game window, in pixels")]
        public int  Width                       { get; set; } = 1280;

        [DisplayName("Height")]
        [Description("Height of the game window, in pixels")]
        public int  Height                      { get; set; } = 720;

        [DisplayName("Borderless Windowed")]
        [Description("If set to true and game is in windowed mode, the window will have no border.")]
        public bool BorderlessWindowed          { get; set; } = true;

        [DisplayName("Resizable Window")]
        [Description("If set to true, the game window can be resized.")]
        public bool ResizableWindowed           { get; set; } = false;

        [DisplayName("High Aspect Ratio Crashfix")]
        [Description("Workaround for borderless windowed that prevents the game crashing on titlecards if the aspect ratio of the game is ridiculous.")]
        public bool HighAspectRatioCrashFix     { get; set; } = true;

        [DisplayName("Aspect Ratio Limit")]
        [Description("If the game window is below this aspect ratio, the widescreen hack scales the window vertically instead of horizontally.")]
        public float AspectRatioLimit           { get; set; } = 4 / 3F;

        [DisplayName("Disable 2P Frameskip")]
        [Description("Disables the frame skipping behaviour in 2 player mode, allowing you to play at 60 FPS.")]
        public bool Disable2PFrameskip          { get; set; } = true;

        [DisplayName("Fast Stage Load Times")]
        [Description("A hack that skips waiting in titlecards, causing levels to load instantly.")]
        public bool StupidlyFastLoadTimes       { get; set; } = true;

        [DisplayName("Default Settings")]
        [Description("The regular settings from the Sonic Heroes launcher.")]
        public DefaultSettings DefaultSettings  { get; set; } = new DefaultSettings();
    }
}
