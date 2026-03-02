using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    public class ReviewProfile : EntityBase
    {
        #region obsolete
        //public string Name { get; set; }
        ///// <summary>
        ///// Начальное значение EF
        ///// </summary>
        //public double StartEF { get; set; }
        ///// <summary>
        ///// Минимальное значение EF
        ///// </summary>
        //public double MinEF { get; set; }
        ///// <summary>
        ///// Максимальное значение EF
        ///// </summary>
        //public double MaxEF { get; set; }
        ///// <summary>
        ///// Моножитель EF для сложности Easy
        ///// </summary>
        //public double EasyRatioEF { get; set; }
        ///// <summary>
        ///// Моножитель EF для сложности Normal
        ///// </summary>
        //public double NormalRatioEF { get; set; }
        ///// <summary>
        ///// Моножитель EF для сложности Hard
        ///// </summary>
        //public double HardRatioEF { get; set; }
        ///// <summary>
        ///// Моножитель EF при забывании
        ///// </summary>
        //public double ErrorRatioEF { get; set; }
        ///// <summary>
        ///// Фиксированный интервал для второго повторения, для первого всегда 1, для последующих по формуле I = round(I * EF)
        ///// </summary>
        //public int SecondRepetitionInterval { get; set; }
        #endregion


        public string Name { get; set; }
        /// <summary>
        /// Моножитель EF для сложности Easy
        /// </summary>
        public double EasyEF { get; set; }
        /// <summary>
        /// Моножитель EF для сложности Normal
        /// </summary>
        public double NormalEF { get; set; }
        /// <summary>
        /// Моножитель EF для сложности Hard
        /// </summary>
        public double HardEF { get; set; }
        /// <summary>
        /// Моножитель EF при забывании
        /// </summary>
        public int EasySecondRepetitionInterval { get; set; }
        public int NormalSecondRepetitionInterval { get; set; }
        public int HardSecondRepetitionInterval { get; set; }

    }
}
