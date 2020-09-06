#include "SnakeGame.h"
#include <Arduino.h>
#include "../Engine.h"

SnakeGame::SnakeGame() {
	snakeLength = 1;
	timeSinceLastMove = 0;

	x[0] = 0;
	y[0] = 3;
	dirX = 1;
	dirY = 0;
	nextDirX = dirX;
	nextDirY = dirY;

	timeRemainingToNextFoodSpawn = 1.5;
	foodExists = false;
	gameOver = false;
	scoreDisplayAmount = -5;
}

void SnakeGame::updateLoop(Engine& engine) {

	//Engine & engine = *ptrEngine;

	if (gameOver) {
		scoreDisplayAmount += engine.deltaTime * 10;
		for (int i = 0; i < min(snakeLength, (int)scoreDisplayAmount); i ++) {
			int x = i % 16;
			int y = i / 16;
			engine.setPixel(x,y);
		}
		return;
	}
	
	timeSinceLastMove += engine.deltaTime;

	// Set next dir to whichever input axis is currently greater
	// This dir is stored for the next time the snake moves
	// (overwriting the value immediately would not allow preventing dir from being reversed)
	if (engine.inputX != 0 || engine.inputY != 0) {
		// Stick is further along x axis than y axis
		if (abs(engine.inputX) > abs(engine.inputY)) {
			int inputDirX = sign(engine.inputX);
			// Dont allow dir to be reversed
			if (inputDirX != -dirX) {
				nextDirX = inputDirX;
				nextDirY = 0;
			}
		}
		// Stick is further along y axis than x axis
		else {
			int inputDirY = sign(engine.inputY);
			// Dont allow dir to be reversed
			if (inputDirY != -dirY) {
				nextDirY = inputDirY;
				nextDirX = 0;
			}
		}
	}
		

	// Move
	if (timeSinceLastMove > timeBetweenMoves) {
		timeSinceLastMove = timeSinceLastMove - timeBetweenMoves;

		dirX = nextDirX;
		dirY = nextDirY;

		// Calculate new head position:
		int newHeadX = x[0] + dirX;
		int newHeadY = y[0] + dirY;

		// Wrap around:
		newHeadX = (newHeadX >= width) ? 0 : newHeadX;
		newHeadX = (newHeadX < 0) ? width-1 : newHeadX;
		newHeadY = (newHeadY >= height) ? 0 : newHeadY;
		newHeadY = (newHeadY < 0) ? height-1 : newHeadY;


		// Update snake points
		for (int i = snakeLength -1; i > 0; i --) {
			x[i] = x[i-1];
			y[i] = y[i-1];

			// Check for self-collision
			if (newHeadX == x[i] && newHeadY == y[i]) {
				gameOver = true;
				engine.playSound(200,1000);
			}
		}
		
		// Move head
		x[0] = newHeadX;
		y[0] = newHeadY;


		// Eat food
		if (foodExists) {
			if (x[0] == foodX && y[0] == foodY) {
				foodExists = false;
				
				// Add point to end of snake
				int nextPointDirX = -dirX;
				int nextPointDirY = -dirY;
				if (snakeLength > 1) {
					nextPointDirX = sign(x[snakeLength-1]-x[snakeLength-2]);
					nextPointDirY = sign(y[snakeLength-1]-y[snakeLength-2]);
				}
				x[snakeLength] = x[snakeLength-1] + nextPointDirX;
				y[snakeLength] = y[snakeLength-1] + nextPointDirY;
				snakeLength ++;

				// Play sound
				if ((snakeLength-1)%5==0) {
					engine.playSound(523,450);
				}
				else {
					engine.playSound(349,150);
				}
			}
		}
	}

	// Draw snake
	for (int i = 0; i < snakeLength; i ++) {
		engine.setPixel((int)x[i], (int)y[i]);
	}

	// Draw food
	if (foodExists) {
		engine.setPixel(foodX, foodY);
	}
	else {
		// Handle food spawning
		timeRemainingToNextFoodSpawn -= engine.deltaTime;
		if (timeRemainingToNextFoodSpawn <= 0) {
			placeFood();
		}
	}

	
	
}

// Place food randomly on tile not currently occupied by snake
void SnakeGame::placeFood() {
	int numTiles = width * height;
	int randomIndex = random(0, numTiles);

	// Create map of tiles occupied by snake
	bool occupancyMap[numTiles] = {false};
	for (int i = 0; i < snakeLength; i ++) {
		int snakeIndex = y[i] * width + x[i];
		occupancyMap[snakeIndex] = true;
	}

	for (int i = 0; i < numTiles; i ++) {
		// Cant spawn food here, tile contains snake
		if (occupancyMap[randomIndex] == true) {
			randomIndex = (randomIndex + 1) % numTiles;

		}
		// Can spawn food here
		else {
			foodX = randomIndex % width;
			foodY = randomIndex / width;
			timeRemainingToNextFoodSpawn = random(foodSpawnMillisMin, foodSpawnMillisMax) / 1000.0;
			foodExists = true;
			return;
		}
	}
}

int SnakeGame::sign(float value) {
	if (value == 0) {
		return 0;
	}
	return (value < 0)?-1:1;
}