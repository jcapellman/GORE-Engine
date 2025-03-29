#pragma once
#include "framework.h"

enum LOGLEVEL {
	DEBUG,
	INFO,
	WARNING,
	ERROR
};

class GoreLogger {
public:
	static GoreLogger& getInstance() {
		static GoreLogger instance;

		return instance;
	}

	void log(LOGLEVEL logLevel, const std::string& message);

	void setLogLevel(LOGLEVEL logLevel);
private:
	GoreLogger() : _currentLogLevel(ERROR) {}

	~GoreLogger() = default;
	std::mutex mutex_;

	LOGLEVEL _currentLogLevel;
};