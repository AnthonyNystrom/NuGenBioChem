namespace System.Windows.Media.Media3D
{
    /// <summary>
    /// Groups transform operations
    /// </summary>
    public static class Matrix3DMethods
    {
        /// <summary>
        /// Appends transform along to the given direction 
        /// (the object must have original direction along YAxis(!))
        /// </summary>
        /// <param name="originalMatrix">Matrix</param>
        /// <param name="direction">Direction</param>
        /// <returns>Transformation</returns>
        public static Matrix3D TransformAlongTo(this Matrix3D originalMatrix, Vector3D direction)
        {
            Matrix3D matrix = new Matrix3D();
            if ((!direction.ApproxEqual(new Vector3D(0,+1,0), 0.001)) && 
                (!direction.ApproxEqual(new Vector3D(0,-1,0), 0.001)))
            {
                Vector3D firstVector = Vector3D.CrossProduct(new Vector3D(0,10,0), direction);
                firstVector.Normalize();

                // Получаем перпендикуляр к полученному ранее перепендикуляру
                Vector3D secondVector = Vector3D.CrossProduct(firstVector, direction);
                secondVector.Normalize();
                
                // Получаем матрицу поворота
                matrix.M11 = firstVector.X; matrix.M12 = firstVector.Y; matrix.M13 = firstVector.Z;
                matrix.M21 = direction.X; matrix.M22 = direction.Y; matrix.M23 = direction.Z;
                matrix.M31 = secondVector.X; matrix.M32 = secondVector.Y; matrix.M33 = secondVector.Z;
            }
            else if (direction.ApproxEqual(new Vector3D(0,-1,0), 0.001))
            {
                matrix.Scale(new Vector3D(1, -1, 1));
            }
            else return originalMatrix;

            // Append transform
            originalMatrix.Append(matrix);
            return originalMatrix;
        }
    }
}
