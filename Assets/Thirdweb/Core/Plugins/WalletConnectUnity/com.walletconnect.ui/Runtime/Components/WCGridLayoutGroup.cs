using UnityEngine;
using UnityEngine.UI;

namespace WalletConnectUnity.UI
{
    // This is a modified version of `GridLayoutGroup` that allows for dynamic horizontal spacing.
    // Most of the code is copied from the original `GridLayoutGroup` implementation because wasn't
    // not possible to extend it efficiently without copying the code.
    // 
    // The only modification is the addition of `_minSpacingX` and `AdjustSpacing()`.
    // Modified code blocks are marked with `// ----- ` comments.
    public class WCGridLayoutGroup : LayoutGroup
    {
        // ----- Added to enable dynamic horizontal spacing
        [SerializeField] private float _minSpacingX = 4;
        private int _cellCountOnX;

        private void AdjustSpacing()
        {
            if (constraint != Constraint.FixedColumnCount)
            {
                // Calculate total width minus padding
                var totalWidth = rectTransform.rect.width - padding.horizontal;

                if (_cellCountOnX > 1)
                {
                    // Calculate spacing based on total width and cell size
                    var requiredWidthForCells = cellSize.x * _cellCountOnX;
                    var availableSpacing = totalWidth - requiredWidthForCells;
                    var spacingGaps = _cellCountOnX - 1;
                    var gap = availableSpacing / spacingGaps;

                    spacing = gap > cellSize.x
                        ? new Vector2(_minSpacingX, this.spacing.y) // don't allow spacing to be larger than cell size
                        : new Vector2(gap, this.spacing.y);
                }
                else
                {
                    // Default to minimum spacing if there's only one cell
                    spacing = new Vector2(_minSpacingX, this.spacing.y);
                }
            }
        }
        // ----- End of modification

        /// <summary>
        /// Which corner is the starting corner for the grid.
        /// </summary>
        public enum Corner
        {
            /// <summary>
            /// Upper Left corner.
            /// </summary>
            UpperLeft = 0,

            /// <summary>
            /// Upper Right corner.
            /// </summary>
            UpperRight = 1,

            /// <summary>
            /// Lower Left corner.
            /// </summary>
            LowerLeft = 2,

            /// <summary>
            /// Lower Right corner.
            /// </summary>
            LowerRight = 3
        }

        /// <summary>
        /// The grid axis we are looking at.
        /// </summary>
        /// <remarks>
        /// As the storage is a [][] we make access easier by passing a axis.
        /// </remarks>
        public enum Axis
        {
            /// <summary>
            /// Horizontal axis
            /// </summary>
            Horizontal = 0,

            /// <summary>
            /// Vertical axis.
            /// </summary>
            Vertical = 1
        }

        /// <summary>
        /// Constraint type on either the number of columns or rows.
        /// </summary>
        public enum Constraint
        {
            /// <summary>
            /// Don't constrain the number of rows or columns.
            /// </summary>
            Flexible = 0,

            /// <summary>
            /// Constrain the number of columns to a specified number.
            /// </summary>
            FixedColumnCount = 1,

            /// <summary>
            /// Constraint the number of rows to a specified number.
            /// </summary>
            FixedRowCount = 2
        }

        [SerializeField] protected Corner m_StartCorner = Corner.UpperLeft;

        /// <summary>
        /// Which corner should the first cell be placed in?
        /// </summary>
        public Corner startCorner
        {
            get { return m_StartCorner; }
            set { SetProperty(ref m_StartCorner, value); }
        }

        [SerializeField] protected Axis m_StartAxis = Axis.Horizontal;

        /// <summary>
        /// Which axis should cells be placed along first
        /// </summary>
        /// <remarks>
        /// When startAxis is set to horizontal, an entire row will be filled out before proceeding to the next row. When set to vertical, an entire column will be filled out before proceeding to the next column.
        /// </remarks>
        public Axis startAxis
        {
            get { return m_StartAxis; }
            set { SetProperty(ref m_StartAxis, value); }
        }

        [SerializeField] protected Vector2 m_CellSize = new Vector2(100, 100);

        /// <summary>
        /// The size to use for each cell in the grid.
        /// </summary>
        public Vector2 cellSize
        {
            get { return m_CellSize; }
            set { SetProperty(ref m_CellSize, value); }
        }

        [SerializeField] protected Vector2 m_Spacing = Vector2.zero;

        /// <summary>
        /// The spacing to use between layout elements in the grid on both axises.
        /// </summary>
        public Vector2 spacing
        {
            get { return m_Spacing; }
            set { SetProperty(ref m_Spacing, value); }
        }

        [SerializeField] protected Constraint m_Constraint = Constraint.Flexible;

        /// <summary>
        /// Which constraint to use for the GridLayoutGroup.
        /// </summary>
        /// <remarks>
        /// Specifying a constraint can make the GridLayoutGroup work better in conjunction with a [[ContentSizeFitter]] component. When GridLayoutGroup is used on a RectTransform with a manually specified size, there's no need to specify a constraint.
        /// </remarks>
        public Constraint constraint
        {
            get { return m_Constraint; }
            set { SetProperty(ref m_Constraint, value); }
        }

        [SerializeField] protected int m_ConstraintCount = 2;

        /// <summary>
        /// How many cells there should be along the constrained axis.
        /// </summary>
        public int constraintCount
        {
            get { return m_ConstraintCount; }
            set { SetProperty(ref m_ConstraintCount, Mathf.Max(1, value)); }
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            constraintCount = constraintCount;
        }

#endif

        /// <summary>
        /// Called by the layout system to calculate the horizontal layout size.
        /// Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            AdjustSpacing();

            int minColumns = 0;
            int preferredColumns = 0;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                minColumns = preferredColumns = m_ConstraintCount;
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                minColumns = preferredColumns =
                    Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
            }
            else
            {
                minColumns = 1;
                preferredColumns = Mathf.CeilToInt(Mathf.Sqrt(rectChildren.Count));
            }

            SetLayoutInputForAxis(
                padding.horizontal + (cellSize.x + spacing.x) * minColumns - spacing.x,
                padding.horizontal + (cellSize.x + spacing.x) * preferredColumns - spacing.x,
                -1, 0);
        }

        /// <summary>
        /// Called by the layout system to calculate the vertical layout size.
        /// Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            int minRows = 0;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                minRows = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                minRows = m_ConstraintCount;
            }
            else
            {
                float width = rectTransform.rect.width;
                int cellCountX = Mathf.Max(1,
                    Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
                minRows = Mathf.CeilToInt(rectChildren.Count / (float)cellCountX);
            }

            float minSpace = padding.vertical + (cellSize.y + spacing.y) * minRows - spacing.y;
            SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
        }

        /// <summary>
        /// Called by the layout system
        /// Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        /// <summary>
        /// Called by the layout system
        /// Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

        private void SetCellsAlongAxis(int axis)
        {
            // Normally a Layout Controller should only set horizontal values when invoked for the horizontal axis
            // and only vertical values when invoked for the vertical axis.
            // However, in this case we set both the horizontal and vertical position when invoked for the vertical axis.
            // Since we only set the horizontal position and not the size, it shouldn't affect children's layout,
            // and thus shouldn't break the rule that all horizontal layout must be calculated before all vertical layout.
            var rectChildrenCount = rectChildren.Count;
            if (axis == 0)
            {
                // Only set the sizes when invoked for horizontal axis, not the positions.

                for (int i = 0; i < rectChildrenCount; i++)
                {
                    RectTransform rect = rectChildren[i];

                    m_Tracker.Add(this, rect,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);

                    rect.anchorMin = Vector2.up;
                    rect.anchorMax = Vector2.up;
                    rect.sizeDelta = cellSize;
                }

                return;
            }

            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;

            int cellCountX = 1;
            int cellCountY = 1;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                cellCountX = m_ConstraintCount;

                if (rectChildrenCount > cellCountX)
                    cellCountY = rectChildrenCount / cellCountX + (rectChildrenCount % cellCountX > 0 ? 1 : 0);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                cellCountY = m_ConstraintCount;

                if (rectChildrenCount > cellCountY)
                    cellCountX = rectChildrenCount / cellCountY + (rectChildrenCount % cellCountY > 0 ? 1 : 0);
            }
            else
            {
                // ----- Modified to use `_minSpacingX`
                if (cellSize.x + spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else
                    cellCountX = Mathf.Max(1,
                        Mathf.FloorToInt(
                            (width - padding.horizontal + _minSpacingX + 0.001f) / (cellSize.x + _minSpacingX)));
                // ----- End of modification

                if (cellSize.y + spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1,
                        Mathf.FloorToInt(
                            (height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }

            int cornerX = (int)startCorner % 2;
            int cornerY = (int)startCorner / 2;

            int cellsPerMainAxis, actualCellCountY;
            if (startAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                _cellCountOnX = Mathf.Clamp(cellCountX, 1, rectChildrenCount);
                actualCellCountY = Mathf.Clamp(cellCountY, 1,
                    Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }

            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, rectChildrenCount);
                _cellCountOnX = Mathf.Clamp(cellCountX, 1,
                    Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }

            Vector2 requiredSpace = new Vector2(
                _cellCountOnX * cellSize.x + (_cellCountOnX - 1) * spacing.x,
                actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
            );
            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
            );

            for (int i = 0; i < rectChildrenCount; i++)
            {
                int positionX;
                int positionY;
                if (startAxis == Axis.Horizontal)
                {
                    positionX = i % cellsPerMainAxis;
                    positionY = i / cellsPerMainAxis;
                }
                else
                {
                    positionX = i / cellsPerMainAxis;
                    positionY = i % cellsPerMainAxis;
                }

                if (cornerX == 1)
                    positionX = _cellCountOnX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;

                SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX,
                    cellSize[0]);
                SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY,
                    cellSize[1]);
            }
        }
    }
}