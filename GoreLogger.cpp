#include "pch.h"

#include "GoreLogger.h"

void GoreLogger::log(LOGLEVEL logLevel, const std::string& message) {
	if (logLevel < _currentLogLevel) {
		return; // Ignore log messages the current log level
	}
	std::lock_guard<std::mutex> lock(mutex_);

	std::ofstream logFile(_logFilePath, std::ios_base::app);

	if (!logFile.is_open()) {
		std::cerr << "Failed to open log file." << std::endl;
		return;
	}

	auto now = std::chrono::system_clock::now();
	auto now_c = std::chrono::system_clock::to_time_t(now);
	std::tm timeInfo;
	localtime_s(&timeInfo, &now_c);

	std::stringstream timeStream;
	timeStream << std::put_time(&timeInfo, "%Y-%m-%d %H:%M:%S");


	logFile << "[" << timeStream.str() << "] ";

	switch (logLevel) {
	case DEBUG:
		logFile << "[DEBUG] ";
		break;
	case INFO:
		logFile << "[INFO] ";
		break;
	case WARN:
		logFile << "[WARNING] ";
		break;
	case ERR:
		logFile << "[ERROR] ";
		break;
	default:
		logFile << "[UNKNOWN] ";
		break;
	}

	logFile << message << std::endl;

	logFile.close();
}

void GoreLogger::setLogLevel(LOGLEVEL logLevel) {
	std::lock_guard<std::mutex> lock(mutex_);

	_currentLogLevel = logLevel;
}

void GoreLogger::setGameRootFolder(const std::string& gameRootFolder) {
	std::lock_guard<std::mutex> lock(mutex_);

	_logFilePath = gameRootFolder + "/" + DEFAULT_LOG_FILE;
}