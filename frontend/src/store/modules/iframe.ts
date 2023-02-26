export default {
	state: {
		iframeList: [],
	},
	mutations: {
		setIframeList(state, route) {
			state.iframeList = [];
			state.iframeList.push(route);
		},
		pushIframeList(state, route) {
			const target = state.iframeList.find(
				(item) => item.path === route.path
			);
			if (!target) {
				state.iframeList.push(route);
			}
		},
		removeIframeList(state, route) {
			state.iframeList.forEach((item, index) => {
				if (item.path === route.path) {
					state.iframeList.splice(index, 1);
				}
			});
		},
		refreshIframe(state, route) {
			state.iframeList.forEach((item) => {
				if (item.path === route.path) {
					const url = route.meta.url;
					item.meta.url = "";
					setTimeout(() => {
						item.meta.url = url;
					}, 200);
				}
			});
		},
		clearIframeList(state) {
			state.iframeList = [];
		},
	},
};
