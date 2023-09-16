import Log from 'consola';
import { switchMap, take, tap } from 'rxjs/operators';
import { catchError, Observable, of } from 'rxjs';
import { AxiosObservable } from 'axios-observable';
import { AxiosError, AxiosResponse } from 'axios';
import ResultDTO from '@dto/ResultDTO';
import { useAlertStore } from '~/store';

export function checkForError<T = any>(
	logText?: string,
	fnName?: string,
	suppressAlert?: boolean,
): (source$: AxiosObservable<ResultDTO<T>>) => Observable<ResultDTO<T>> {
	return (source$) =>
		source$.pipe(
			catchError((error: AxiosError | any) => {
				const alertStore = useAlertStore();
				Log.error('FATAL NETWORK ERROR: ', error);

				const url = new URL(error.config.url, error.config.baseURL);
				if (!suppressAlert) {
					alertStore.showAlert({
						id: 0,
						title: error.message,
						text: `Failed a request to url: ${url}`,
						result: JSON.parse(error.config.data),
					});
				}

				// TODO Check wat the error contains in-case, of network failure and continue based on that
				return of({
					data: { isSuccess: false, isFailed: true, errors: [{ message: error.message }] } as ResultDTO,
				} as AxiosResponse<ResultDTO<T>>);
			}),
			switchMap((response: AxiosResponse<ResultDTO<T>>): Observable<ResultDTO<T>> => {
				return of(response?.data);
			}),
			tap((data) => Log.trace(`${logText}${fnName} response:`, data)),
			// Ensure we complete any API calls after the response has been received
			take(1),
		);
}
