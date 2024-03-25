import { Observable, of, throwError } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import type { PlexMediaDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto';
import type ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';
import { PLEX_LIBRARY_RELATIVE_PATH, PLEX_MEDIA_RELATIVE_PATH } from '@api-urls';
import ApiClient from '@dto/ApiClient';

export function getTvShow(id: number): Observable<ResultDTO<PlexMediaSlimDTO>> {
	return PlexRipperAxios.get<PlexMediaSlimDTO>({
		url: `${PLEX_MEDIA_RELATIVE_PATH}/tvshow/${id}`,
		apiCategory: '',
		apiName: getTvShow.name,
	});
}

export function getMediaDetailByIdEndpoint(plexMediaId: number, type: PlexMediaType): Observable<PlexMediaDTO> {
	return from(
		ApiClient.plexMedia.getMediaDetailByIdEndpoint(plexMediaId, {
			type,
		}),
	).pipe(
		switchMap(({ data }) => {
			if (data.isFailed) {
				return throwError(() => data.errors);
			}
			if (data.value) {
				return of(data.value);
			}

			return throwError(() => 'No value returned');
		}),
	);
}

export function getLibraryMediaData(id: number, page: number, size: number): Observable<ResultDTO<PlexMediaSlimDTO[]>> {
	return PlexRipperAxios.get<PlexMediaSlimDTO[]>({
		url: `${PLEX_LIBRARY_RELATIVE_PATH}/${id}/media`,
		params: { page, size },
		apiCategory: '',
		apiName: getLibraryMediaData.name,
	});
}

export function getThumbnail(plexMediaId: number, plexMediaType: PlexMediaType, width = 0, height = 0): Observable<any> {
	return PlexRipperAxios.getImage<Blob>({
		url: `${PLEX_MEDIA_RELATIVE_PATH}/thumb`,
		params: {
			plexMediaId,
			plexMediaType,
			width,
			height,
		},
		responseType: 'blob',
	});
}
