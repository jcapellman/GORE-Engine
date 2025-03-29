#pragma once
#include "framework.h"

enum LOGLEVEL {
	DEBUG,
	INFO,
	WARN,
	ERR
};

const LOGLEVEL DEFAULT_LOG_LEVEL = DEBUG;
const std::string DEFAULT_LOG_FILE = "gconsole.log";

class GoreLogger {
public:
	static GoreLogger& getInstance() {
		static GoreLogger instance;

		return instance;
	}

	void log(LOGLEVEL logLevel, const std::string& message);

	void setLogLevel(LOGLEVEL logLevel);
	void setGameRootFolder(const std::string& gameRootFolder);
private:
	GoreLogger() : _currentLogLevel(DEFAULT_LOG_LEVEL), _logFilePath(DEFAULT_LOG_FILE) {}
	~GoreLogger() = default;
	
	// Prevent copying and assignment
	GoreLogger(const GoreLogger&) = delete;
	GoreLogger& operator=(const GoreLogger&) = delete;

	std::mutex mutex_;

	LOGLEVEL _currentLogLevel;
	std::string _logFilePath;
};