import Log from 'consola';
import { Observable } from 'rxjs';
import { NotificationDTO } from '@dto/mainApi';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { BaseService, GlobalService, SignalrService } from '@service';
import { getNotifications, hideNotification } from '@api/notificationApi';
import IStoreState from '@interfaces/IStoreState';
import { Context } from '@nuxt/types';

export class NotificationService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					notifications: state.notifications,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		GlobalService.getAxiosReady()
			.pipe(
				tap(() => Log.debug('Retrieving all notifications')),
				switchMap(() => getNotifications()),
			)
			.subscribe((result) => {
				if (result.isSuccess) {
					this.setState({ notifications: result.value }, 'Set Notifications');
				}
			});

		SignalrService.getNotificationUpdates().subscribe((data) => {
			this.setState({ notifications: [...this.getState().notifications, ...[data]] }, 'Add Notifications');
		});
	}

	// region Notifications

	public getNotifications(): Observable<NotificationDTO[]> {
		return this.stateChanged.pipe(map((state: IStoreState) => state?.notifications ?? []));
	}

	public hideNotification(id: number): void {
		const notifications = this.getState().notifications;
		const notificationIndex = notifications.findIndex((x) => x.id === id);
		if (notificationIndex > -1) {
			notifications[notificationIndex].hidden = true;
			this.setState({ notifications }, 'Set Notification Hidden');
		}
		hideNotification(id).pipe(take(1)).subscribe();
	}
	// endregion
}

const notificationService = new NotificationService();
export default notificationService;