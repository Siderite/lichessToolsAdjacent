using System;
using System.Collections.Generic;
using System.Text;

namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Generates icons from the Lichess sfd file
    /// https://github.com/lichess-org/lila/blob/master/public/font/lichess.sfd
    /// </summary>
    public interface IIconGenerator
    {
        /// <summary>
        /// Generate the js and css files for the icons
        /// </summary>
        /// <returns></returns>
        Task Generate();
    }
}
