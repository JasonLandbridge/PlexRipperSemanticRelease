import { seed as falsoSeed } from '@ngneat/falso';
import type { MockConfig } from '@mock';
import { PlexMediaType } from '@dto';

let currentSeed = 0;

export function checkConfig(config: Partial<MockConfig> = {}): MockConfig {
	if (config === null || config === undefined) {
		return checkConfig({});
	}
	const defaultConfig: MockConfig = {
		plexServerCount: 5,
		seed: 1234,
		debugDisplayData: false,
		plexAccountCount: 1,
		firstTimeSetup: false,
		plexServerAccessCount: 3,
		plexServerStatusCount: 3,
		plexLibraryTypes: [PlexMediaType.Movie, PlexMediaType.TvShow],
		movieDownloadTask: 5,
		tvShowDownloadTask: 5,
		seasonDownloadTask: 5,
		episodeDownloadTask: 5,
		connectionHasProgress: false,
		setDownloadDetails: false,
		maxServerConnections: 5,
		plexMovieLibraryCount: 2,
		plexTvShowLibraryCount: 2,
		movieCount: 0,
		tvShowCount: 0,
		seasonCount: 0,
		episodeCount: 0,
		folderPathCount: 0,
		override: {
			plexServer: (plexServers) => plexServers,
			plexServerConnections: (plexServerConnections) => plexServerConnections,
			plexLibraries: (plexLibraries) => plexLibraries,
			plexAccounts: (plexAccounts) => plexAccounts,
			downloadTasks: (downloadTasks) => downloadTasks,
			settings: (settings) => settings,
		},
	};

	for (const key in defaultConfig) {
		if (!Object.hasOwn(config, key)) {
			config[key] = defaultConfig[key];
		} else if (typeof defaultConfig[key] === 'object' && defaultConfig[key] !== null) {
			config[key] = { ...defaultConfig[key], ...config[key] };
		}
	}

	setSeed(config?.seed ?? defaultConfig.seed);

	return config as MockConfig;
}

export function setSeed(seed: number) {
	currentSeed = seed;
	falsoSeed('' + seed);
}

export function incrementSeed(value = 1): void {
	falsoSeed('' + currentSeed + value);
}
