using Heroes.Graphics.Essentials.Config.Structures;

namespace Heroes.Graphics.Essentials.Config
{
    public class Config
    {
        public int  Width                       { get; set; } = 1280;
        public int  Height                      { get; set; } = 720;
        public bool BorderlessWindowed          { get; set; } = true;
        public bool ResizableWindowed           { get; set; } = false;

        public bool HighAspectRatioCrashFix     { get; set; } = true;
        public float AspectRatioLimit           { get; set; } = 4 / 3F;

        public bool Disable2PFrameskip          { get; set; } = true;

        public bool StupidlyFastLoadTimes       { get; set; } = true;
        public DefaultSettings DefaultSettings  { get; set; } = new DefaultSettings();
    }
}
