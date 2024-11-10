import { route } from '@fixtures/baseE2E';
import { JobStatus, MessageTypes, type ServerConnectionCheckStatusProgressDTO } from '@dto';

describe('Check server connections dialog', () => {
	before(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 3,
			maxServerConnections: 3,
		});
		cy.visit(route('/empty'));
	});

	it('Should display the check server connections dialog when given the back-end signal', function () {
		cy.getPageData().then((data) => {
			cy.hubPublishCheckPlexServerConnectionsJob(JobStatus.Started, data.plexServers, data.plexServerConnections);
		});

		cy.log('Should display the servers when the account has access to those Plex servers');

		cy.getCy('check-server-connection-dialog').should('exist').and('be.visible');

		cy.get('.q-card-dialog-content')
			.getPageData()
			.then((data) => {
				// Ensure the dialog is displaying the correct number of servers
				for (const plexServer of data.plexServers) {
					cy.findByText(plexServer.name, {
						selector: '[data-cy="check-server-connections-dialog-server-title"]',
					})
						.should('exist')
						.and('be.visible');
					// Ensure the dialog is displaying the correct number of connections per server
					const connections = data.plexServerConnections.filter((x) => x.plexServerId === plexServer.id);
					for (const plexServerConnection of connections) {
						cy.findByText(plexServerConnection.url, {
							selector: '[data-cy="check-server-connections-dialog-connection-title"]',
						})
							.should('exist')
							.and('be.visible');
					}
				}
			});

		cy.getPageData().then((data) => {
			for (const plexServerConnection of data.plexServerConnections) {
				cy.wait(500).hubPublish('progress', MessageTypes.ServerConnectionCheckStatusProgress, {
					plexServerId: plexServerConnection.plexServerId,
					plexServerConnectionId: plexServerConnection.id,
					connectionSuccessful: true,
					statusCode: 200,
					completed: true,
				} as Partial<ServerConnectionCheckStatusProgressDTO>);
			}
		});

		cy.getCy('check-server-connection-dialog-hide-btn').click();
		cy.getCy('check-server-connection-dialog').should('not.exist');
	});
});
