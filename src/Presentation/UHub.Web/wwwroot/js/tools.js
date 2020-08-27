/**
 * 发送提示消息，若用户同意弹出桌面通知，则优先使用桌面通知
 * @param {string} message 消息
 * @param {number} type 成功消息 1, 失败消息 -1
 */
function showMessage(message, type) {
	var icon = "/images/notify/success.png";
	if (type < 0) {
		icon = "/images/notify/failure.png";
	}
	var options = {
		dir: "auto", // 文字方向
		body: message, // 通知主体
		requireInteraction: true, // 不自动关闭通知
		// 通知图标
		icon: icon
	};

	// 先检查浏览器是否支持
	if (!window.Notification) {
		//console.log('浏览器不支持通知');
		simpleMsg(message, type);
	} else {
		// 检查用户曾经是否同意接受通知
		if (Notification.permission === 'granted') {
			var notification = new Notification(message, options); // 显示通知
		} else if (Notification.permission === 'default') {
			// 用户还未选择，可以询问用户是否同意发送通知
			Notification.requestPermission().then(permission => {
				if (permission === 'granted') {
					//console.log('用户同意授权');
					var notification = new Notification(message, options); // 显示通知
				} else if (permission === 'default') {
					//console.warn('用户关闭授权 未刷新页面之前 可以再次请求授权');
					simpleMsg(message, type);
				} else {
					// denied
					//console.log('用户拒绝授权 不能显示通知');
					simpleMsg(message, type);
				}
			});
		} else {
			// denied 用户拒绝
			//console.log('用户曾经拒绝显示通知');
			simpleMsg(message, type);
		}
	}
}

/**
 * 显示消息提示框
 * @param {string} message 消息
 * @param {number} type 成功消息 1, 失败消息 -1
 */
function simpleMsg(message, type) {
	switch (type) {
		case -1:
			toastr.error(message, '失败！');
			break;
		case 1:
			toastr.success(message, '成功！');
			break;
	}
}