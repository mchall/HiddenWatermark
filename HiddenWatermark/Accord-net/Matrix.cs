// Accord Math Library
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright © César Souza, 2009-2015
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace Accord.Math
{
    using System;
    using System.Collections.Generic;

    public static partial class Matrix
    {

        /// <summary>
        ///   Returns a sub matrix extracted from the current matrix.
        /// </summary>
        /// 
        /// <param name="source">The matrix to return the submatrix from.</param>
        /// <param name="startRow">Start row index</param>
        /// <param name="endRow">End row index</param>
        /// <param name="startColumn">Start column index</param>
        /// <param name="endColumn">End column index</param>
        /// 
        public static T[,] Submatrix<T>(this T[,] source,
            int startRow, int endRow, int startColumn, int endColumn)
        {
            return submatrix(source, null, startRow, endRow, startColumn, endColumn);
        }

        /// <summary>
        ///   Extracts a selected area from a matrix.
        /// </summary>
        /// 
        /// <remarks>
        ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
        /// </remarks>
        /// 
        private static T[,] submatrix<T>(this T[,] source, T[,] destination,
            int startRow, int endRow, int startColumn, int endColumn)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            int rows = source.GetLength(0);
            int cols = source.GetLength(1);

            if ((startRow > endRow) || (startColumn > endColumn) || (startRow < 0) ||
                (startRow >= rows) || (endRow < 0) || (endRow >= rows) ||
                (startColumn < 0) || (startColumn >= cols) || (endColumn < 0) ||
                (endColumn >= cols))
            {
                throw new ArgumentException("Argument out of range.");
            }

            if (destination == null)
                destination = new T[endRow - startRow + 1, endColumn - startColumn + 1];

            for (int i = startRow; i <= endRow; i++)
                for (int j = startColumn; j <= endColumn; j++)
                    destination[i - startRow, j - startColumn] = source[i, j];

            return destination;
        }

        /// <summary>
        ///   Pads a matrix by filling all of its sides with zeros.
        /// </summary>
        /// 
        /// <param name="matrix">The matrix whose contents will be padded.</param>
        /// <param name="bottom">How many rows to add at the bottom.</param>
        /// <param name="top">How many rows to add at the top.</param>
        /// <param name="left">How many columns to add at the left side.</param>
        /// <param name="right">How many columns to add at the right side.</param>
        /// 
        /// <returns>The original matrix with an extra row of zeros at the selected places.</returns>
        /// 
        public static T[,] Pad<T>(this T[,] matrix, int top, int right, int bottom, int left)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            T[,] r = (T[,])Array.CreateInstance(typeof(T), rows + top + bottom, cols + left + right);

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    r[i + top, j + left] = matrix[i, j];

            return r;
        }

        /// <summary>
        ///   Gets a row vector from a matrix.
        /// </summary>
        public static T[] GetRow<T>(this T[,] m, int index)
        {
            T[] row = new T[m.GetLength(1)];

            for (int i = 0; i < row.Length; i++)
                row[i] = m[index, i];

            return row;
        }

        /// <summary>
        ///   Stores a row vector into the given row position of the matrix.
        /// </summary>
        public static T[,] SetRow<T>(this T[,] m, int index, T[] row)
        {
            for (int i = 0; i < row.Length; i++)
                m[index, i] = row[i];

            return m;
        }

        /// <summary>
        ///   Gets a column vector from a matrix.
        /// </summary>
        public static T[] GetColumn<T>(this T[,] m, int index)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            if (index >= cols)
                throw new ArgumentOutOfRangeException("index");

            T[] column = new T[rows];

            for (int i = 0; i < column.Length; i++)
                column[i] = m[i, index];

            return column;
        }

        /// <summary>
        ///   Stores a column vector into the given column position of the matrix.
        /// </summary>
        public static T[,] SetColumn<T>(this T[,] m, int index, T[] column)
        {
            for (int i = 0; i < column.Length; i++)
                m[i, index] = column[i];

            return m;
        }
    }
}