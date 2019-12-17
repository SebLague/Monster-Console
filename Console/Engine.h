#pragma once
#include <Arduino.h>
#include <LedControl.h>


class Engine {
private:

	// Input pins:
	const int buttonPin = 4;
	const int xPin = 0;
	const int yPin = 2;

	// Audio pins:
	const int buzzerPin = 11;
	
	// Display pins:
	const int dinPin = 9;
	const int csPin = 8;
	const int clkPin = 10;

	LedControl ledController = LedControl(dinPin,clkPin,csPin,2); // (DIN, CLK, CS, num displays

	unsigned char rowsDisplayA[8];
	unsigned char rowsDisplayB[8];
	int buttonState;

	float remap(float, float, float, float, float);

public:
	float deltaTime;
	unsigned long time;

	// Player input info:
	float inputX;
	float inputY;
	bool buttonDown;
	bool buttonUpThisFrame;
	bool buttonDownThisFrame;
	float buttonDownDuration;


	Engine();

	void playSound(int frequency, int duration);

	void updateLoop(float);

	void clearScreen();

	void setPixel(int x, int y);

	void setPixelToValue(int x, int y, bool on);

	void drawToDisplay();

	void setDisplayBrightness(int);
	
};