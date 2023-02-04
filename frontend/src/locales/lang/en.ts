import "dayjs/locale/en";

export default {
	requestError: {
		400: "There was an error in the request. The server did not create or modify data.",
		401: "User has no permissions (wrong token, username, password).",
		403: "The user is authorized, but access is forbidden.",
		404: "The requested resource does not exist.",
		406: "The requested format is not available.",
		410: "The requested resource has been permanently deleted and will not be available again.",
		422: "A validation error occurred while creating an object.",
		429: "The request is too frequent, please try again later.",
		500: "Internal Server Error.",
		502: "Gateway error.",
		503: "Service unavailable, server temporarily overloaded or maintained.",
		504: "Gateway timed out.",
		default: "Request timeout.",
		requestError: "Request error",
		fileError: "File download failed or the file does not exist",
	},
	message: {
		title: "Hint:",
		systemInit: "Please wait patiently while the system is initialized",
		hostError: "Unknown host access!",
		webSiteError: "Unknown site, please confirm that the site is correct!",
	},
	common: {
		searchButton: "search",
		resetButton: "reset",
		addButton: "add",
		editButton: "edit",
		removeButton: "delete",
		batchRemoveButton: "batch Remove",
		detailButton: "detail",
	},
	model: {
		user: "user",
		org: "org",
		pos: "pos",
		role: "role",
		bizUser: "bizUser",
	},
	login: {
		loginSuccess: "Login successfully",
		reLogin: "Restart login",
		loginInvalidation: "The login is invalid, please login again",
		signInTitle: "Sign in",
		forgetPassword: "Forget password",
		signIn: "Sign in",
		signInOther: "Sign in with",
		accountPlaceholder: "Please input a user account",
		accountError: "Please input a user account",
		PWPlaceholder: "Please input a password",
		PWError: "Please input a password",
		validLaceholder: "Please input a valid",
		validError: "Please input a valid",
		accountPassword: "Account Password",
		phoneSms: "Phone SMS",
		phonePlaceholder: "Please input a phone",
		smsCodePlaceholder: "Please input a SMS code",
		getSmsCode: "SMS code",
		machineValidation: "Machine Validation",
		sendingSmsMessage: "Sending SMS Message",
		newPwdPlaceholder: "Please input a new password",
		backLogin: "Back Login",
		restPassword: "Rest Password",
		emailPlaceholder: "Please input a email",
		emailCodePlaceholder: "Please input a Email code",
		restPhoneType: "For phone rest",
		restEmailType: "For email rest",
	},
};
