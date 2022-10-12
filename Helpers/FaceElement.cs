namespace UnpackMiColorFace
{
    internal class FaceElement
    {
        public uint TargetId { get; internal set; }
        public int PosX { get; internal set; }
        public int PosY { get; internal set; }

        public FaceElement()
        { }

        public FaceElement(uint targetId, int posX = 0, int posY = 0)
        {
            this.TargetId = targetId;
            this.PosX = posX;
            this.PosY = posY;
        }
    }
}