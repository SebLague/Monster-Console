#pragma once
#include "Game.h"
#include "../Engine.h"

class SnakeGame : public Game {

private:
	static const int width = 16;
	static const int height = 8;
	const float timeBetweenMoves = .2;
	const int foodSpawnMillisMin = 400;
	const int foodSpawnMillisMax = 2000;

	unsigned char x[width * height];
	unsigned char y[width * height];

	int snakeLength;
	float timeSinceLastMove;

	int dirX;
	int dirY;
	int nextDirX;
	int nextDirY;

	bool foodExists;
	float timeRemainingToNextFoodSpawn;
	int foodX;
	int foodY;

	bool gameOver;
	float scoreDisplayAmount;

	void placeFood();
	int sign(float);

public:
	SnakeGame();
	virtual void updateLoop(Engine&);
};