#include "Engine.h"
#include "Arduino.h"
#include <LedControl.h>
#include "src/Game.h"

Engine :: Engine() {
	ledController.shutdown(0, false);
	ledController.shutdown(1, false);

	pinMode(buttonPin, INPUT);
  	pinMode(buzzerPin, OUTPUT);

  	clearScreen();
  	time = 0;
}

void Engine :: updateLoop(float deltaTime) {
	this->deltaTime = deltaTime;
	time += deltaTime;

	// Button input:
	int buttonStateOld = buttonState;
	buttonState = digitalRead(buttonPin);
	buttonDown = buttonState == 1;
  	buttonDownThisFrame = buttonDown && buttonState != buttonStateOld;
  	buttonUpThisFrame = buttonState == 0 && buttonStateOld == 1;

  	if (buttonDownThisFrame) {
  		buttonDownDuration = 0;
  	}
  	if (buttonDown) {
  		buttonDownDuration += deltaTime;
  	}

	// Get analog stick input:
	const float inputThreshold = 0.1;
	inputX = remap(analogRead(xPin), 0, 1023, -1, 1);
	inputY = -remap(analogRead(yPin), 0, 1023, -1, 1);

  Serial.print("X: ");
  Serial.print(inputX);
  Serial.print(" Y: ");
  Serial.print(inputY);
  Serial.print("\n");

	if (abs(inputX) < inputThreshold) {
		inputX = 0;
	}
	if (abs(inputY) < inputThreshold) {
		inputY = 0;
	}
}

void Engine :: playSound(int frequency, int duration) {
	
	tone(buzzerPin, frequency, duration);
}

void Engine :: clearScreen() {
 	for (int i = 0; i < 8; i ++) {
      rowsDisplayA[i] = 0;
      rowsDisplayB[i] = 0;
 	}
}

void Engine :: drawToDisplay() {
	for (int row = 0; row < 8; row ++) {
		ledController.setRow(0, row, rowsDisplayA[row]);
		ledController.setRow(1, row, rowsDisplayB[row]);
 	}
}


// set display brightness [0,15]
void Engine :: setDisplayBrightness(int brightness) {
	ledController.setIntensity(0, brightness);
    ledController.setIntensity(1, brightness);
}

void Engine :: setPixel(int x, int y) {
	setPixelToValue(x, y, true);
}

void Engine :: setPixelToValue(int x, int y, bool on) {
	if (x >= 16 || x < 0 || y >= 8 || y < 0) {
		return;
	}

	if (x >= 8) {
		if (on) {
			rowsDisplayA[x-8] |= 1 << y;
		}
		else {
			rowsDisplayA[x-8] &= ~(1 << y);
		}
	}
	else {
		if (on) {
			rowsDisplayB[x] |= 1 << y;
		}
		else {
			rowsDisplayB[x] &= ~(1 << y);
		}
	}
}

float Engine :: remap(float value, float minOld, float maxOld, float minNew, float maxNew) {
  return minNew + (value - minOld)/(maxOld-minOld) * (maxNew-minNew);
}
