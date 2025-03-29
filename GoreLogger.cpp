#include "pch.h"

#include "GoreLogger.h"

void GoreLogger::log(LOGLEVEL logLevel, const std::string& message) {
	std::lock_guard<std::mutex> lock(mutex_);

	std::ofstream logFile(DEFAULT_LOG_FILE, std::ios_base::app);

	if (!logFile.is_open()) {
		std::cerr << "Failed to open log file." << std::endl;
		return;
	}

	switch (logLevel) {
	case DEBUG:
		logFile << "[DEBUG] ";
		break;
	case INFO:
		logFile << "[INFO] ";
		break;
	case WARNING:
		logFile << "[WARNING] ";
		break;
	case ERROR:
		logFile << "[ERROR] ";
		break;
	default:
		logFile << "[UNKNOWN] ";
		break;
	}

	logFile << message << std::endl;

	logFile.close();
}