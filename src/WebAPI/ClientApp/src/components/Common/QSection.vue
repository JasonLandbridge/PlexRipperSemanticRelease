<template>
	<QRow
		no-gutters
		style="width: 100%">
		<QCol cols="12">
			<!-- Header	-->
			<QRow
				v-if="$slots['header']"
				no-gutters>
				<QCol>
					<QText
						size="h5"
						class="q-my-sm"
						align="center"
						bold="medium">
						<slot name="header" />
					</QText>
					<q-separator />
				</QCol>
			</QRow>
			<!--	Section Content	-->
			<slot />
		</QCol>
	</QRow>
</template>
