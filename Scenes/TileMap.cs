using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class TileMap : Godot.TileMap {

	Vector2I[] empty_cells = Array.Empty<Vector2I>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		var cells = GetUsedCells(0);
		foreach(var cell in cells) {
			var data = GetCellTileData(0, cell);
			if((bool)data.GetCustomData("isEmpty")) {
				empty_cells = empty_cells.Append(cell).ToArray();
			}
		}
	}

	public Vector2 getRandomEmptyCell() {
		if (empty_cells.Length == 0) {
			GD.PrintErr("No empty cells available!");
			return Vector2.Zero; // Return a default value if no empty cells are found
		}

		Random random = new Random();
		int index = random.Next(empty_cells.Length);
		Vector2 global_pos = ToGlobal(MapToLocal(empty_cells[index]));
		return global_pos;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}
}
