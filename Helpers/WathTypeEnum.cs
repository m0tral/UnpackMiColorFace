﻿namespace UnpackMiColorFace
{
    internal enum WatchType
    {
        Undefined = 0,
        /// <summary>
        /// Mi Watch, Mi Color, Mi Revolve
        /// </summary>
        Gen1 = 1,
        /// <summary>
        /// Mi Color2, Mi Watch S1, Mi Watch S1 Active
        /// </summary>
        Gen2 = 3,
        /// <summary>
        /// Mi Watch S1 Pro
        /// </summary>
        Gen3 = 4,
        /// <summary>
        /// Redmi Watch 2, Poco Watch
        /// </summary>
        Redmi = 5,
        /// <summary>
        /// Mi Band 7 Pro
        /// </summary>
        Band7Pro = 6,
    }
}