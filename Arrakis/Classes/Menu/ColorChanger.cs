/*
 * Arrakis | Classes/Menu/ColorChanger.cs
 *
 * Copyright (C) 2026 Arrakis
 * https://github.com/nauth-studios/Arrakis
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using UnityEngine;

namespace Arrakis.Classes
{
    public class ColorChanger : MonoBehaviour
    {
        public void Start()
        {
            if (colors == null)
            {
                Destroy(this);
                return;
            }
            targetRenderer = GetComponent<Renderer>();
            if (colors.IsFlat())
            {
                Update();
                Destroy(this);
                return;
            }
            Update();
        }
        public void Update()
        {
            targetRenderer.enabled = !colors.transparent;
            if (colors.transparent)
                return;
            targetRenderer.material.color = colors.GetCurrentColor();
        }
        public Renderer targetRenderer;
        public ExtGradient colors;
    }
}